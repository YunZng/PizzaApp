#nullable disable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaApp.Areas.Identity.Data;
using PizzaApp.Data;
using PizzaApp.Models;

namespace PizzaApp.Pages.Pizzas
{

    public class CreatePizzaData {
        public string Name { get; set; } = default!;
        public string Price { get; set; }
    }
    public class CreateModel : PageModel
    {
        private readonly PizzaDbContext _context;
        private readonly UserManager<PizzaIdentityUser> _userManager;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(PizzaDbContext context, UserManager<PizzaIdentityUser> userManager, ILogger<CreateModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public CreatePizzaData Pizza { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("call me");
            PizzaIdentityUser user = await _userManager.GetUserAsync(User);
            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid");
                return Page();
            }
            _context.Pizza.Add(new Pizza{Name = Pizza.Name, Price = Pizza.Price, Company = user.Company});
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
