using User.Data.Access.Helpers.DTO.UserProfile;
using User.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace User.API.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [Authorize(Roles = "Admin, User")]
    [HttpGet("GetUserProfile")]
    public async Task<IActionResult> GetUserProfileAsync()
    {
        var userProfileId = new Guid(User.FindFirst("Id").Value);

        return Ok(await _userProfileService.GetUserProfileByIdAsync(userProfileId));
    }

    [Authorize(Roles = "Admin, User")]
    [HttpPut("EditUserProfile")]
    public async Task<IActionResult> EditUserProfileAsync([FromForm] UserProfileDto userProfile, [FromQuery] bool updateDataFromCV)
    {
        var userProfileId = new Guid(User.FindFirst("Id").Value);

        await _userProfileService.EditUserProfileByIdAsync(userProfileId, userProfile, updateDataFromCV);

        var message = new { message = "You have successfully edited your profile!" };

        return Ok(message);
    }

    [Authorize(Roles = "Admin, User")]
    [HttpPost("RecommendJobs")]
    public async Task<IActionResult> RecommendJobsAsync()
    {
        var userProfileId = new Guid(User.FindFirst("Id").Value);

        await _userProfileService.SendRecommandJobsMessageAsync(userProfileId);

        return Ok();
    }
}
