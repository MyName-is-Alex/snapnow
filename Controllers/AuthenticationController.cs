using System.Net.Http.Headers;
using Azure.Core;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using snapnow.DTOS;
using snapnow.Models;
using snapnow.Services;

namespace snapnow.Controllers;

[ApiController]
[Route("api/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    public AuthenticationController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUserRoute([FromForm]RegisterUserModel userModel)
    {
        var result = await _userService.RegisterUser(userModel, Url);
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return Problem(
            statusCode: result.StatusCode,
            title: result.Message,
            detail: result.Errors != null ? String.Join("/n", result.Errors) : ""
        ); 
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUserRoute([FromForm] LoginUserModel userModel)
    {
        var response = await _userService.LoginUser(userModel);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        
        return Problem(
            statusCode: response.StatusCode,
            title: response.Message,
            detail: response.Errors != null ? String.Join("/n", response.Errors) : ""
        );
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult LogoutUserRoute()
    {
        var response = _userService.Logout();
        if (response.IsSuccess)
        {
            return Ok(response);
        }

        return Problem(
            statusCode: response.StatusCode,
            title: response.Message,
            detail: response.Errors != null ? String.Join("/n", response.Errors) : ""
        );
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string Id, string token)
    {
        if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(token))
        {
            return Problem(
                title: "Email NOT confirmed.",
                detail: "Some required email confirmation information was not specified.",
                statusCode: 400
            );
        }

        var confirmEmailResponse = await _userService.ConfirmUserEmail(Id, token);
        
        if (!confirmEmailResponse.IsSuccess)
        {
            return Problem(
                title: confirmEmailResponse.Message,
                detail: confirmEmailResponse.Errors != null ? String.Join("/n", confirmEmailResponse.Errors) : "",
                statusCode: confirmEmailResponse.StatusCode
            );
        }
        return Ok(confirmEmailResponse);
    }
    
    [HttpPost("google")]
    public async Task<IActionResult> AuthenticateWithGoogle([FromBody] AccessTokenModel userAuthorizationResponse)
    {
        var accessToken = userAuthorizationResponse.AccessToken;

        if (accessToken == null)
        {
            return BadRequest("Access token is missing.");
        }
        
        var googleAuthenticationResponse = await _userService.GoogleAuthentication(accessToken);

        if (googleAuthenticationResponse.IsSuccess)
        {
            return Ok(googleAuthenticationResponse);
        }

        return Problem(
            title: googleAuthenticationResponse.Message,
            detail: googleAuthenticationResponse.Errors != null
                ? String.Join("/n", googleAuthenticationResponse.Errors)
                : "",
            statusCode: googleAuthenticationResponse.StatusCode
        );
        /*using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response =
                await client.GetAsync("https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses");
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest();
            }

            var content = await response.Content.ReadAsStringAsync();
            var userProfile = JsonConvert.DeserializeObject<UserInfoGoogleModel>(content);

            return Ok(userProfile);
        }*/
    }
}