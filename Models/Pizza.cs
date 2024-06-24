using System.ComponentModel.DataAnnotations;

namespace PizzaApp.Models;

public class Pizza
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required, RegularExpression(@"([0-9]|[1-9][0-9]*)?(\.[0-9]{0,2})?")]
    public string Price { get; set; } = "0.00";
}