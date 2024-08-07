using System.ComponentModel.DataAnnotations;

public class Invitation
{
  [Key]
  public string Token { get; set; } = default!;
  public string Email { get; set; } = default!;
  public string Role { get; set; } = default!;
  public string Company { get; set; } = default!;
  public DateTime ExpiryDate { get; set; } = default!;
  public bool IsUsed { get; set; } = default!;
}
