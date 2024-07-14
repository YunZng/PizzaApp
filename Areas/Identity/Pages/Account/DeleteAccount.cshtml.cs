

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaApp.Areas.Identity.Data;

namespace PizzaApp.Areas.Identity.Pages.Account;

[Authorize(Roles = "Owner,Manager")]
public class DeleteAccountModel : PageModel
{
  private readonly UserManager<PizzaIdentityUser> _userManager;
  private readonly ILogger<DeleteAccountModel> _logger;
  public string userEmail { get; set; } = default!;
  public DeleteAccountModel(
    UserManager<PizzaIdentityUser> userManager,
    ILogger<DeleteAccountModel> logger
    )
  {
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<IActionResult> OnGetAsync(string email)
  {
    _logger.LogInformation("Delete account");
    Console.WriteLine("Delete account");
    if (email == null)
    {
      _logger.LogWarning("Email is null");
      return NotFound();
    }
    var selectedUser = await _userManager.FindByEmailAsync(email);
    if (selectedUser == null)
    {
      _logger.LogError("User not found");
      return NotFound();
    }
    userEmail = email;
    return Page();
  }

  public async Task<IActionResult> OnPostAsync(string email)
  {
    _logger.LogInformation("Delete account");
    Console.WriteLine("Delete account");
    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid");
      return Page();
    }
    var selectedUser = await _userManager.FindByEmailAsync(email);
    if (selectedUser == null)
    {
      _logger.LogError("User not found");
      return NotFound();
    }
    await _userManager.DeleteAsync(selectedUser);
    return RedirectToPage("./AccountList");
  }

}