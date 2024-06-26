using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public DbSet<PizzaApp.Models.Pizza> Pizza { get; set; } = default!;
    }
}
