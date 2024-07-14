#nullable disable
using Microsoft.AspNetCore.Identity;
using PizzaApp.Areas.Identity.Data;
using PizzaApp.Data;

namespace PizzaApp.Services;
public class InvitationService
{
    private readonly UserManager<PizzaIdentityUser> _userManager;
    private readonly PizzaDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InvitationService(UserManager<PizzaIdentityUser> userManager, PizzaDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> EmailTaken(string email){
        var user = await _userManager.FindByEmailAsync(email);
        return user != null;
    }

    public async Task<string> GenerateInvitationLink(string email, string role, string company, TimeSpan expiryDuration)
    {
        var emailTaken = await EmailTaken(email);
        if (emailTaken)
        {
            throw new Exception("An account with that email already exists.");
        }
        var token = Guid.NewGuid().ToString();
        var expiryDate = DateTime.UtcNow.Add(expiryDuration);

        Invitation invitation = new Invitation
        {
            Email = email,
            Role = role,
            Company = company,
            Token = token,
            ExpiryDate = expiryDate,
            IsUsed = false
        };

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        var domain = _httpContextAccessor.HttpContext.Request.Host.Value;
        var link = $"http://{domain}/Identity/Account/Register?token={token}";
        return link;
    }
}
