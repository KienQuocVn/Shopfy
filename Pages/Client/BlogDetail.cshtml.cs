using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shofy.Pages.Client;

public class Pages_Client_Blog_DetailModel : PageModel
{
    private readonly ILogger<Pages_Client_Blog_DetailModel> _logger;

    public Pages_Client_Blog_DetailModel(ILogger<Pages_Client_Blog_DetailModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}