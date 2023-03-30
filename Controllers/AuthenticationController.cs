using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using snapnow.DTOS;
using snapnow.Services;

namespace snapnow.Controllers;

[ApiController]
[Route("api/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthenticationController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUserRoute([FromForm]RegisterUserModel userModel)
    {
        var result = await _userService.RegisterUser(userModel);
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
        var response = await _userService.LoginUser(userModel, HttpContext);
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

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutUserRoute()
    {
        var response = await _userService.Logout(HttpContext);
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
}