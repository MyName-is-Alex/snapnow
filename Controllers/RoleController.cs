using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using snapnow.DAOS;
using snapnow.DTOS;
using snapnow.Services;

namespace snapnow.Controllers;

[Authorize]
[ApiController]
[Route("api/role")]
public class RoleController : ControllerBase
{
    private readonly RoleService _roleService;

    public RoleController(RoleService roleService)
    {
        _roleService = roleService;
    }
    
    [HttpPost("add-roles")]
    public async Task<IActionResult> AddNewRole([FromForm]RoleModel roleModel)
    {
        var response = await _roleService.Add(roleModel);
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

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("aaaaa");
    }
}