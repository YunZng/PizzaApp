using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PizzaApp.Areas.Identity.Data;

// Add profile data for application users by adding properties to the PizzaIdentityUser class
public class PizzaIdentityUser : IdentityUser
{
  [PersonalData]
  public string Company { get; set; } = default!;
}