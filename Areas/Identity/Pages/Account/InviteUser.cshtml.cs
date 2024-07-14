#nullable disable

using System.ComponentModel.DataAnnotations;
using ContactManager.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using PizzaApp.Areas.Identity.Data;
using PizzaApp.Services;
using WebPWrecover.Services;

namespace PizzaApp.Areas.Identity.Pages.Account;
public class InviteUserModel : PageModel
{
  public string ReturnUrl { get; set; }
  private readonly SignInManager<PizzaIdentityUser> _signInManager;
  private readonly UserManager<PizzaIdentityUser> _userManager;
  private readonly RoleManager<IdentityRole> _roleManager;
  private readonly IUserStore<PizzaIdentityUser> _userStore;
  private readonly IUserEmailStore<PizzaIdentityUser> _emailStore;
  private readonly ILogger<RegisterModel> _logger;
  private readonly IEmailSender _emailSender;
  private AuthMessageSenderOptions Options { get; }
  private readonly InvitationService _invitationService;
  public string Role { get; set; } = Constants.Staff;
  public string StatusMessage { get; set; } = default!;
  public string DisplayStatus { get; set; } = ".d-none";

  public List<SelectListItem> Roles { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = Constants.Staff, Text = Constants.Staff  },
            new SelectListItem { Value = Constants.Manager, Text = Constants.Manager },
        };
  public InviteUserModel(
      UserManager<PizzaIdentityUser> userManager,
      RoleManager<IdentityRole> roleManager,
      ILogger<RegisterModel> logger,
      IEmailSender emailSender,
      IOptions<AuthMessageSenderOptions> optionsAccessor,
      InvitationService invitationService)
  {
    _userManager = userManager;
    _roleManager = roleManager;
    _logger = logger;
    _emailSender = emailSender;
    Options = optionsAccessor.Value;
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
  // public async Task OnGetAsync(string returnUrl = null)
  // {
  // }

  public async Task<IActionResult> OnPostAsync(string returnUrl = null)
  {
    var user = await _userManager.GetUserAsync(User);
    var company = user.Company;
    try
    {
      var link = await _invitationService.GenerateInvitationLink(Input.Email, Role, company, TimeSpan.FromDays(1));
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