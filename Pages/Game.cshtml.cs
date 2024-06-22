using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BattleShip.Pages
{
    public class GameModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string RoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Nickname { get; set; }

        public void OnGet()
        {
        }
    }
}
