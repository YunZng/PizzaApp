using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Areas.Identity.Data;
using PizzaApp.Models;

namespace PizzaApp.Data
{
    public class PizzaDbContext : IdentityDbContext<PizzaIdentityUser>
    {
        public PizzaDbContext (DbContextOptions<PizzaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pizza> Pizza { get; set; } = default!;
        public DbSet<Invitation> Invitations { get; set; } = default!;
    }
}
