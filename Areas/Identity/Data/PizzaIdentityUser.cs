using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PizzaApp.Areas.Identity.Data;

// Add profile data for application users by adding properties to the PizzaIdentityUser class
public class PizzaIdentityUser : IdentityUser
{
  [Required]
  public string Company { get; set; } = default!;
}