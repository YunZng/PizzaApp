#nullable disable

using ContactManager.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Areas.Identity.Data;
using PizzaApp.Data;


namespace PizzaApp.Areas.Identity.Pages.Account;

public class UserRoles
{
  public string Id { get; set; } = default!;
  public string Username { get; set; } = default!;
  public string Email { get; set; } = default!;
  public IList<string> Roles { get; set; } = default!;
}
[Authorize(Roles = "Owner,Manager")]
public class AccountListModel : PageModel
{
  private readonly PizzaDbContext _context;
  private readonly UserManager<PizzaIdentityUser> _userManager;
  public IList<UserRoles> userRoles;
  // Inject the database context to get user account data
  // Inject the user manager to get user account roles, this way we don't have to create custom identity classes, and just have to stick with the existing IdentityRole class.
  public AccountListModel(PizzaDbContext context, UserManager<PizzaIdentityUser> userManager)
  {
    _context = context;
    _userManager = userManager;
    userRoles = new List<UserRoles>();
  }
  public IList<PizzaIdentityUser> Users { get; set; } = default!;
  // When page first launched, read throught the database, get user email and roles.
  // If the account is the app owner, get every account.
  // If the account is admin/manager, get every account created by this user.
  public async Task OnGetAsync()
  {
    PizzaIdentityUser loggedInUser = await _userManager.GetUserAsync(User);
    Users = await _context.Users.Where(user => user.Company == loggedInUser.Company && user.Email != loggedInUser.Email).ToListAsync();
    foreach (var user in Users)
    {
      if (!await _userManager.IsInRoleAsync(user, Constants.Owner))
      {
        IList<string> roles = await _userManager.GetRolesAsync(user);
        userRoles.Add(new UserRoles { Id = user.Id, Username = user.UserName, Email = user.Email!, Roles = roles });
      }
    }
  }
}