using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Models;

namespace PizzaApp.Pages.Pizzas
{
    public class DeleteModel : PageModel
    {
        private readonly PizzaApp.Data.PizzaDbContext _context;

        public DeleteModel(PizzaApp.Data.PizzaDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Pizza Pizza { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pizza = await _context.Pizza.FirstOrDefaultAsync(m => m.Id == id);

            if (pizza == null)
            {
                return NotFound();
            }
            else
            {
                Pizza = pizza;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pizza = await _context.Pizza.FindAsync(id);
            if (pizza != null)
            {
                Pizza = pizza;
                _context.Pizza.Remove(Pizza);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
