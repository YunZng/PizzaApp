using System.ComponentModel.DataAnnotations;

namespace ContactManager.Authorization
{
  public class Constants
  {
    [Display(Name = "Owner")]
    public static readonly string Owner = "Owner";
    [Display(Name = "Manager")]
    public static readonly string Manager = "Manager";
    [Display(Name = "Staff")]
    public static readonly string Staff = "Staff";
  }
}