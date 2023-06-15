using BabyPedia.Data;
using BabyPedia.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BabyPedia.Controllers;

public class ChatController:Controller
{
    
    private readonly BabyPediaContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ChatController> _logger;

    public ChatController(BabyPediaContext dbContext, UserManager<IdentityUser> userManager, ILogger<ChatController> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }


    [HttpPost("/chat/{userId}")]
    public async Task<IActionResult> RecordChat([FromQuery] string message, string userId)
    {
        var currentSignedInUser = await _userManager.GetUserAsync(User);
        var targetUser = await _userManager.FindByIdAsync(userId);

        await _dbContext.Chats.AddAsync(new Chat()
        {
            To = targetUser, From = currentSignedInUser, Content = message 
        });

        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation($"Chat to {targetUser.UserName}: {message}");
        
        return Content("OK!");
    }
}