#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaApp.Areas.Identity.Data;
namespace PizzaApp.Areas.Identity.Pages.Account;

[Authorize(Roles = "Owner,Manager")]
public class EditAccountModel : PageModel
{
  private readonly UserManager<PizzaIdentityUser> _userManager;
  private readonly ILogger<EditAccountModel> _logger;
  // private PizzaIdentityUser selectedUser = default!;

  [BindProperty]
  public string userEmail { get; set; } = default!;

  public EditAccountModel(
    UserManager<PizzaIdentityUser> userManager,
    ILogger<EditAccountModel> logger
  )
  {
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<IActionResult> OnGetAsync(string email)
  {
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
    await _userManager.SetEmailAsync(selectedUser, userEmail);
    await _userManager.SetUserNameAsync(selectedUser, userEmail);
    return RedirectToPage("./AccountList");
  }
}