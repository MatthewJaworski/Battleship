using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BattleShip.Pages
{
    public class GameListModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Nickname { get; set; }
    }
}
