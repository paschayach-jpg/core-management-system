using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services
{
    public static class GlobalData
    {
        public static List<string>     CombinedList  { get; set; }
        public static string           Token         { get; set; }
        public static List<MenuAction> MenuActions   { get; private set; }
        public static UserMenuModel    UserMenuModel { get; set; }

        public static void SetMenuActions(List<MenuAction> menuActions)
        {
            MenuActions = menuActions;
        }
    }
}
