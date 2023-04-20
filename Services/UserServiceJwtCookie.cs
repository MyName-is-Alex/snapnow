using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using snapnow.DAOS;
using snapnow.DTOS;
using snapnow.ErrorHandling;
using snapnow.Models;
using snapnow.Utils;


namespace snapnow.Services;

public class UserServiceJwtCookie : IUserService
{
    private readonly IUserDao _userDao;
    private readonly RoleService _roleDao;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMailService _mailService;

    public UserServiceJwtCookie(IUserDao userDao, RoleService roleDao, IConfiguration configuration, 
        IHttpContextAccessor httpContextAccessor, IMailService mailService)
    {
        _userDao = userDao;
        _roleDao = roleDao;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _mailService = mailService;
    }

    public async Task<IBaseResponse> RegisterUser(RegisterUserModel userModel, IUrlHelper urlHelper)
    {
        var userResponseModel = new UserResponseModel();

        if (userModel == null)
        {
            throw new NullReferenceException();
        }

        if (passwordDoesNotMatch(ref userResponseModel, userModel.Password, userModel.ConfirmPassword))
            return userResponseModel;
        
        var databaseResponse = _userDao.CheckPhoneNumber(userModel.PhoneNumber);
        var isUniquePhoneNumber = databaseResponse.Result;
        if (!databaseResponse.IsSuccess)
            return databaseResponse;

        if (!isUniquePhoneNumber)
        {
            databaseResponse.Message = "The phone number is already in use.";
            databaseResponse.IsSuccess = false;
            return databaseResponse;
        }

        var identityUser = new ApplicationUser
        {   
            Email = userModel.Email,
            UserName = userModel.Email,
            PhoneNumber = userModel.PhoneNumber
        };
        
        var addUserResponse = await _userDao.Add(identityUser, userModel.Password);
        if (!addUserResponse.IsSuccess)
            return addUserResponse;

        if (!addUserResponse.Result!.Succeeded)
        {
            addUserResponse.Message = "User could not be added to database.";
            addUserResponse.IsSuccess = false;
            return addUserResponse;
        }

        await AddUserToRole(identityUser, userModel.Role);

        // if everything ok, create email confirmation token
        var emailConfirmationLinkResponse = await GetEmailConfirmationLink(identityUser, urlHelper);
        if (!emailConfirmationLinkResponse.IsSuccess)
        {
            emailConfirmationLinkResponse.Message +=
                " // User was added to database.";
            return emailConfirmationLinkResponse;
        }

        var mailRequest = new MailRequest
        {
            ToEmail = identityUser.Email,
            Subject = "Confirm your email - snapnow.",
            Body = EmailBody.GetEmailConfirmationBody(identityUser.Email, emailConfirmationLinkResponse.Result!),
        };
        var sendEmailConfirmationResponse = await _mailService.SendEmailAsync(mailRequest);
        if (!sendEmailConfirmationResponse.IsSuccess)
        {
            sendEmailConfirmationResponse.Message += " // User was added to the database.";
            return sendEmailConfirmationResponse;
        }

        if (string.IsNullOrEmpty(sendEmailConfirmationResponse.Token))
        {
            sendEmailConfirmationResponse.Message =
                "Could not send email confirmation link. // User was registered successfully.";
            sendEmailConfirmationResponse.IsSuccess = false;
            return sendEmailConfirmationResponse;
        }

        return new UserResponseModel
        {
            Message = "User was registered successfully",
            StatusCode = 200,
            IsSuccess = true,
            ConfirmationLink = emailConfirmationLinkResponse.Result
        };
    }

    public async Task<IBaseResponse> LoginUser(LoginUserModel userModel)
    {
        var databaseResponse = await _userDao.GetByNameIdentifier(userModel.Email);
        if (!databaseResponse.IsSuccess) 
            return databaseResponse;
        
        var user = databaseResponse.Result;

        if (user == null)
        {
            databaseResponse.Message = "Email or password is invalid.";
            databaseResponse.IsSuccess = false;
            return databaseResponse;
        }
        
        var checkPasswordResponse = await _userDao.CheckPassword(user, userModel.Password);
        if (!checkPasswordResponse.IsSuccess)
            return checkPasswordResponse;
        
        var correctPassword = checkPasswordResponse.Result;

        if (!correctPassword)
        {
            checkPasswordResponse.Message = "Email or password is invalid.";
            checkPasswordResponse.IsSuccess = false;
            return checkPasswordResponse;
        }

        var userRolesResponse = await _userDao.GetRolesForUser(user);
        if (!userRolesResponse.IsSuccess)
            return userRolesResponse;
        
        var userRoles = String.Join(",", userRolesResponse.Result!);
        
        var userCanSignInResponse = await _userDao.UserConfirmedEmail(user);
        if (!userCanSignInResponse.IsSuccess)
            return userCanSignInResponse;

        if (!userCanSignInResponse.Result)
        {
            userCanSignInResponse.Message = "You must confirm your email.";
            userCanSignInResponse.IsSuccess = false;
            return userCanSignInResponse;
        }
        
        // if everything is ok the user is signed in
        string tokenAsString = generateJwtToken(userModel.Email, userRoles);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(10),
            IsEssential = true,
            Secure = true
        };

        _httpContextAccessor.HttpContext!.Response.Cookies.Append("Token", tokenAsString, cookieOptions);
        
        userCanSignInResponse.Message = "Cookie was set.";
        userCanSignInResponse.IsSuccess = true;
        return new UserResponseModel
        {
            Message = "Cookie was set.",
            IsSuccess = true,
            StatusCode = userCanSignInResponse.StatusCode,
            Errors = userCanSignInResponse.Errors,
            Token = tokenAsString
        };
    }

    public async Task<IBaseResponse> GoogleAuthentication(string accessToken)
    {
        // ask google server for user info using the access token.
        UserInfoGoogleModel userProfile;
        
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response =
                await client.GetAsync("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses");
            if (!response.IsSuccessStatusCode)
            {
                return new UserResponseModel
                {
                    Message = "The google Uri or access token is invalid.",
                    StatusCode = (int)response.StatusCode,
                    IsSuccess = false
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            userProfile = JsonConvert.DeserializeObject<UserInfoGoogleModel>(content)!;
        }
        // check if the user already exists in the database.
        var userEmail = userProfile.EmailAddresses[0].Value;
        var userExistsResponse = await _userDao.GetByNameIdentifier(userEmail);

        if (!userExistsResponse.IsSuccess)
        {
            return userExistsResponse;
        }
        ApplicationUser? user = userExistsResponse.Result;
        
        // if user don't exists in the database, register the user without specifying a password and also save to userlogins table;
        if (user == null)
        {
            user = new ApplicationUser
            {
                Email = userEmail,
                UserName = userEmail
            };
            var addUserResponse = await _userDao.Add(user);

            if (!addUserResponse.IsSuccess)
            {
                return addUserResponse;
            }

            if (!addUserResponse.Result!.Succeeded)
            {
                addUserResponse.Message = "User could not be registered.";
                addUserResponse.IsSuccess = false;
                return addUserResponse;
            }
            var loginInfo = new UserLoginInfo("Google", "", "Google");
            var addUserLoginResponse = await _userDao.AddUserLogin(user, loginInfo);
            if (!addUserLoginResponse.IsSuccess)
            {
                addUserLoginResponse.Message =
                    "User was added to the database. // Could not add login provider to 'UserLogin' table.";
                return addUserLoginResponse;
            }

            if (!addUserLoginResponse.Result!.Succeeded)
            {
                addUserLoginResponse.Message +=
                    "User was added to the database. // Could not add login provider to 'UserLogin' table. // The Jwt token will nto be created.";
                addUserLoginResponse.IsSuccess = false;
                return addUserLoginResponse;
            }

            await AddUserToRole(user, "User");
        }
        var userRolesResponse = await _userDao.GetRolesForUser(user);
        if (!userRolesResponse.IsSuccess)
            return userRolesResponse;
        
        var userRoles = String.Join(",", userRolesResponse.Result!);

        // create a jwt token for the user
        var tokenAsString = generateJwtToken(user.Email, userRoles);
        var cookieOptions = new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(10),
            IsEssential = true,
            Secure = false,
            Domain = "localhost"
        };
        
        // use HttpContext to save it into the cookies
        _httpContextAccessor.HttpContext!.Response.Cookies.Append("Token", tokenAsString, cookieOptions);
        
        // return the response to the controller
        return new UserResponseModel
        {
            Message = "Cookie was set.",
            IsSuccess = true,
            StatusCode = 200,
            Token = tokenAsString
        };
    }

    public IBaseResponse Logout()
    {
        _httpContextAccessor.HttpContext!.Response.Cookies.Delete("Token");

        return new UserResponseModel
        {
            Message = "User logged out successfully.",
            IsSuccess = true,
            StatusCode = 200
        };
    }

    public async Task<DatabaseResponseModel<ApplicationUser>> GetUserById(string userId)
    {
        var userResponse = await _userDao.GetById(userId);
        if (!userResponse.IsSuccess)
        {
            return userResponse;
        }

        if (userResponse.Result == null)
        {
            userResponse.Message = "Could not find user.";
            userResponse.IsSuccess = false;
            return userResponse;
        }

        return userResponse;
    }

    public async Task<IBaseResponse> AddUserToRole(ApplicationUser user, string role)
    {
        var roleResponse = await _roleDao.GetBy(role);
        if (!roleResponse.IsSuccess)
        {
            roleResponse.Message += " // User was added to db.";
            return roleResponse;
        }
        
        var defaultUser = roleResponse.Result;
        if (defaultUser == null)
        {
            roleResponse.Message = "User was added to database. // User was not assigned to a role";
            return roleResponse;
        }

        var addToRoleResponse = await _userDao.AddUserToRole(user, defaultUser.Name);
        if (!addToRoleResponse.IsSuccess)
        {
            addToRoleResponse.Message =
                "User was added to database. // User was not assigned to a role. // Could not connect to database.";
            return addToRoleResponse;
        }

        if (!addToRoleResponse.Result!.Succeeded)
        {
            addToRoleResponse.Message =
                "User was added to database. // User was not assigned to a role. // Could not connect to database.";
            addToRoleResponse.IsSuccess = true;
            return addToRoleResponse;
        }

        return addToRoleResponse;
    }
    
    private string generateJwtToken(string email, string roles)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, roles)
        };

        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["AuthSettings:Issuer"],
            audience: _configuration["AuthSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(10),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private async Task<DatabaseResponseModel<string>> GetEmailConfirmationLink(ApplicationUser user, IUrlHelper urlHelper) 
    { 
        var tokenResponse = await _userDao.GenerateConfirmationToken(user);
        if (!tokenResponse.IsSuccess)
            return tokenResponse;

        var token = tokenResponse.Result;

        if (token == null)
        {
            tokenResponse.Message = "Could not generate token.";
            tokenResponse.IsSuccess = false;
            return tokenResponse;
        }

        var confirmationLink = urlHelper.Action(
            "ConfirmEmail", "Authentication",
            new { user.Id, token },
            _httpContextAccessor.HttpContext!.Request.Scheme
        );

        if (confirmationLink == null)
        {
            tokenResponse.Message = "Could not generate link.";
            tokenResponse.IsSuccess = false;
            return tokenResponse;
        }
        
        tokenResponse.Message = "Token was generated successfully.";
        tokenResponse.IsSuccess = true;
        tokenResponse.StatusCode = 200;
        tokenResponse.Result = confirmationLink;

        return tokenResponse;
    }

    public async Task<IBaseResponse> ConfirmUserEmail(string userId, string token)
    {
        var userResponse = await _userDao.GetById(userId);
        
        if (!userResponse.IsSuccess)
        {
            return userResponse;
        }

        if (userResponse.Result == null)
        {
            userResponse.Message = "Could not find user.";
            userResponse.IsSuccess = false;
            return userResponse;
        }
        
        var confirmEmailResponse = await _userDao.ConfirmUserEmail(userResponse.Result, token);
        if (!confirmEmailResponse.IsSuccess)
        {
            return confirmEmailResponse;
        }

        if (!confirmEmailResponse.Result!.Succeeded)
        {
            confirmEmailResponse.Message = "Email was not confirmed.";
            confirmEmailResponse.IsSuccess = false;
            return confirmEmailResponse;
        }

        confirmEmailResponse.Message = "Thank you for confirming your email address.";
        return confirmEmailResponse;
    }

    private bool passwordDoesNotMatch(ref UserResponseModel userResponseModel, string password1, string password2)
    {
        if (password1 != password2)
        {
            userResponseModel.Message = "Passwords does not match.";
            userResponseModel.IsSuccess = false;
            userResponseModel.StatusCode = 422;
            userResponseModel.Errors = new List<string> { "Password doesn't match." };
            return true;
        }

        return false;
    }
}