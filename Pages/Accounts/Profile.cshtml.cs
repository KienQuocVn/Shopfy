using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shofy.Pages.Acounts;

public class ProfileModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public ProfileModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}