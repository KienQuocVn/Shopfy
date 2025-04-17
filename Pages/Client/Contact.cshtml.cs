using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shofy.Pages.Client;

public class Pages_Client_ContactModel : PageModel
{
    private readonly ILogger<Pages_Client_ContactModel> _logger;

    public Pages_Client_ContactModel(ILogger<Pages_Client_ContactModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}