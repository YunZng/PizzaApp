#nullable disable

using System.ComponentModel.DataAnnotations;
using ContactManager.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PizzaApp.Areas.Identity.Data;
using PizzaApp.Data;
using PizzaApp.Services;

namespace PizzaApp.Areas.Identity.Pages.Account;
[Authorize(Roles = "Owner,Manager")]
public class InviteUserModel : PageModel
{
  public string ReturnUrl { get; set; }
  public PizzaDbContext _context { get; set; }
  private readonly UserManager<PizzaIdentityUser> _userManager;
  private readonly ILogger<RegisterModel> _logger;
  private readonly IEmailSender _emailSender;
  private readonly InvitationService _invitationService;
  public string StatusMessage { get; set; } = default!;
  public string DisplayStatus { get; set; } = ".d-none";

  public List<SelectListItem> Roles { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = Constants.Staff, Text = Constants.Staff, Selected = true },
            new SelectListItem { Value = Constants.Manager, Text = Constants.Manager },
        };
  public InviteUserModel(
      PizzaDbContext context,
      UserManager<PizzaIdentityUser> userManager,
      ILogger<RegisterModel> logger,
      IEmailSender emailSender,
      InvitationService invitationService)
  {
    _context = context;
    _userManager = userManager;
    _logger = logger;
    _emailSender = emailSender;
    _invitationService = invitationService;
  }

  [BindProperty]
  public InputModel Input { get; set; }
  public class InputModel
  {
    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; }

    [Required]
    [EmailAddress, RegularExpression(@"^[a-zA-Z0-9._+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,9}$")]
    [Display(Name = "Email")]
    public string Email { get; set; }
  }

  public async Task<IActionResult> OnPostAsync(string returnUrl = null)
  {
    var user = await _userManager.GetUserAsync(User);
    var company = (await _context.Companies.FindAsync(Guid.Parse(user.Company))).Name;

    try
    {
      var link = await _invitationService.GenerateInvitationLink(Input.Email, Input.Role, user.Company, TimeSpan.FromDays(1));
      _logger.LogInformation("{link} generated", link);
      await _emailSender.SendEmailAsync(Input.Email, "Invitation to Register", $"You have been invited to join {company} as a {Input.Role}. Please follow this link to create your account within 24 hours {link}");
      StatusMessage = "Your invitation has been sent.";
      DisplayStatus = "";
      return Page();
    }
    catch (Exception)
    {
      return Page();
    }
  }
}