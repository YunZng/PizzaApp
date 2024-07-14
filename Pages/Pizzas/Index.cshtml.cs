#nullable disable
using ContactManager.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Areas.Identity.Data;
using PizzaApp.Models;

namespace PizzaApp.Pages.Pizzas;
public class IndexModel : PageModel
{
    private readonly Data.PizzaDbContext _context;
    private readonly UserManager<PizzaIdentityUser> _userManager;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(Data.PizzaDbContext context, UserManager<PizzaIdentityUser> userManager, ILogger<IndexModel> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public IList<Pizza> Pizza { get; set; } = new List<Pizza>();

    public async Task OnGetAsync()
    {
        var pizzas = await _context.Pizza.ToListAsync();
        PizzaIdentityUser user = await _userManager.GetUserAsync(User);
        if (await _userManager.IsInRoleAsync(user, Constants.Owner))
        {
            Pizza = pizzas;
        }
        else
        {
            foreach (var pizza in pizzas)
            {
                if (pizza.AdminGroup == user.Company || pizza.AdminGroup == user.Email)
                {
                    Pizza.Add(new Pizza
                    {
                        Id = pizza.Id,
                        Name = pizza.Name,
                        Price = pizza.Price,
                        AdminGroup = pizza.AdminGroup
                    });
                }
            }
        }
    }
}
