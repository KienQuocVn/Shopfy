using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shofy.Pages.Client;

public class Pages_Client_BlogModel : PageModel
{
    private readonly ILogger<Pages_Client_BlogModel> _logger;

    public Pages_Client_BlogModel(ILogger<Pages_Client_BlogModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}