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
    public IActionResult LoginUserRoute([FromForm]LoginUserModel userModel)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("logout")]
    public IActionResult LogoutUserRoute()
    {
        throw new NotImplementedException();
    }
}