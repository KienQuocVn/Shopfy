using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shofy.Pages.Client;

public class Pages_Client_AboutModel : PageModel
{
    private readonly ILogger<Pages_Client_AboutModel> _logger;

    public Pages_Client_AboutModel(ILogger<Pages_Client_AboutModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}