using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shofy.Pages.Client;

public class Pages_Client_IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public Pages_Client_IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}