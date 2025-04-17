using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shofy.Pages.Client;

public class Pages_Client_Shoping_CartModel : PageModel
{
    private readonly ILogger<Pages_Client_Shoping_CartModel> _logger;

    public Pages_Client_Shoping_CartModel(ILogger<Pages_Client_Shoping_CartModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}