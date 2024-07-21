using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PizzaApp.Pages.Reports;

[Authorize(Roles = "Owner,Manager")]
public class IndexModel : PageModel
{
  public IndexModel() { }
  // public async Task OnGetAsync(){}
}