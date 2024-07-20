// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using ContactManager.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PizzaApp.Areas.Identity.Data;
using PizzaApp.Data;
using WebPWrecover.Services;

namespace PizzaApp.Areas.Identity.Pages.Account;
public class RegisterModel : PageModel
{
    private readonly PizzaDbContext _context;
    private readonly SignInManager<PizzaIdentityUser> _signInManager;
    private readonly UserManager<PizzaIdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserStore<PizzaIdentityUser> _userStore;
    private readonly IUserEmailStore<PizzaIdentityUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;
    private AuthMessageSenderOptions Options { get; }
    public bool Disabled { get; set; } = false;
    [BindProperty(SupportsGet = true)]
    public string Token { get; set; }
    public static Invitation Invitation { get; set; }

    public RegisterModel(
        PizzaDbContext pizzaDbContext,
        UserManager<PizzaIdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUserStore<PizzaIdentityUser> userStore,
        SignInManager<PizzaIdentityUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender,
        IOptions<AuthMessageSenderOptions> optionsAccessor)
    {
        _context = pizzaDbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
        Options = optionsAccessor.Value;
    }

    [BindProperty]
    public InputModel Input { get; set; }
    public string ReturnUrl { get; set; }
    public IList<AuthenticationScheme> ExternalLogins { get; set; }
    public class InputModel
    {

        [Required]
        [Display(Name = "Company")]
        public string Company { get; set; }

        [Required]
        [EmailAddress, RegularExpression(@"^[a-zA-Z0-9._+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,9}$")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(63, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 14)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }


    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        _logger.LogInformation("token is " + Token);
        Invitation = await _context.Invitations.FindAsync(Token);
        if (Invitation != null)
        {
            if (Invitation.IsUsed || Invitation.ExpiryDate < DateTime.Now)
            {
                RedirectToPage("/Identity/Account/InvalidToken");
            }
            var companyName = (await _context.Companies.FindAsync(Guid.Parse(Invitation.Company))).Name;
            Input = new InputModel
            {
                Company = companyName,
                Email = Invitation.Email,
            };
            Disabled = true;
        }
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        // If invitation detected, prefill the form
        if (Invitation != null)
        {
            if (Invitation.IsUsed || Invitation.ExpiryDate < DateTime.Now)
            {
                RedirectToPage("/Identity/Account/InvalidToken");
            }
            var companyName = (await _context.Companies.FindAsync(Guid.Parse(Invitation.Company))).Name;
            Input.Company = companyName;
            Input.Email = Invitation.Email;
            Disabled = true;
            ModelState.Remove("Input.Company");
            ModelState.Remove("Input.Email");
        }

        if (ModelState.IsValid)
        {
            // Use owner role by default.
            var role = Disabled ? Invitation.Role : Constants.Owner;

            // check if company name exists, if not create one
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Name == Input.Company);
            if (company == null)
            {
                company = new Company { Name = Input.Company };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            var user = CreateUser();
            user.Company = company.Id.ToString();
            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId, code, returnUrl },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    $"You are registering an account at {Input.Company} for the {role} role.\n\nPlease confirm your account by clicking this link: {callbackUrl}");

                // Create role in database if it doesn't exist.
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                // Role is beign assigned
                await _userManager.AddToRoleAsync(user, role);

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { Input.Email, returnUrl = returnUrl });
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        // If we got this far, something failed, redisplay form
        return Page();
    }

    private PizzaIdentityUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<PizzaIdentityUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(PizzaIdentityUser)}'. " +
                $"Ensure that '{nameof(PizzaIdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<PizzaIdentityUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<PizzaIdentityUser>)_userStore;
    }
}