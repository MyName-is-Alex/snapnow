using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using snapnow.Services;
using System.Security.Claims;

namespace snapnow.Controllers;
[Authorize]
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    
    public ChatController(IChatService userService)
    {
        _chatService = userService;
    }

    [HttpPost("get-followed-users")]
    public async Task<IActionResult> GetFollowedUsers()
    {
        var userName = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;

        if (userName == null)
        {
            return Problem(statusCode: 401,
                title: "Could not retrieve user Id",
                detail: "User might not be logged in.");
        }
        var result = await _chatService.GeFollowedUsers(userName);

        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return Problem(statusCode: result.StatusCode,
            title: result.Message,
            detail: result.Errors != null ? String.Join("/n", result.Errors) : "");
    }
}