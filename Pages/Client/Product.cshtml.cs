using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shofy.Pages.Client;

public class Pages_Client_ProductModel : PageModel
{
    private readonly ILogger<Pages_Client_ProductModel> _logger;

    public Pages_Client_ProductModel(ILogger<Pages_Client_ProductModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}