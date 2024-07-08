// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using ContactManager.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using PizzaApp.Areas.Identity.Data;
using WebPWrecover.Services;

namespace PizzaApp.Areas.Identity.Pages.Account;
public class RegisterModel : PageModel
{
    private readonly SignInManager<PizzaIdentityUser> _signInManager;
    private readonly UserManager<PizzaIdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserStore<PizzaIdentityUser> _userStore;
    private readonly IUserEmailStore<PizzaIdentityUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;
    private AuthMessageSenderOptions Options { get; }

    public RegisterModel(
        UserManager<PizzaIdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUserStore<PizzaIdentityUser> userStore,
        SignInManager<PizzaIdentityUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender,
        IOptions<AuthMessageSenderOptions> optionsAccessor)
    {
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
        [RegularExpression(@"^[A-Za-z][A-Za-z0-9_]{7,29}$")]
        [Display(Name = "Username")]
        public string Username { get; set; }

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
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            // Use owner role by default.
            var ownerExists = await _roleManager.RoleExistsAsync(Constants.OwnerRole);
            var role = ownerExists ? Constants.AdminRole : Constants.OwnerRole;
            var email = Options.AppOwnerEmail;
            var confirmationEmail = email;
            var createdBy = ownerExists ? email : "self";
            var loggedInUserRole = role;
            PizzaIdentityUser loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser != null)
            {
                confirmationEmail = loggedInUser.Email;
                email = Input.Email;
                createdBy = loggedInUser.Email;
                loggedInUserRole = (await _userManager.GetRolesAsync(loggedInUser))[0].ToString();
                if (loggedInUserRole == Constants.OwnerRole)
                {
                    role = Constants.AdminRole;
                }
                else if (loggedInUserRole == Constants.AdminRole)
                {
                    role = Constants.StaffRole;
                }
                _logger.LogInformation(loggedInUser.ToString() + loggedInUserRole + "User added to role: " + role);
            }

            var user = CreateUser();
            user.CreatedBy = createdBy;
            await _userStore.SetUserNameAsync(user, Input.Username, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(confirmationEmail, "Confirm your email",
                    $"A registration request from {email} for {role} role.\nPlease confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
                // Role is beign assigned
                await _userManager.AddToRoleAsync(user, role);
                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email, returnUrl = returnUrl });
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