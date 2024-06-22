using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BattleShip.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string Nickname { get; set; }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Nickname))
            {
                return Page();
            }

            return RedirectToPage("/GameList", new { nickname = Nickname });
        }
    }
}