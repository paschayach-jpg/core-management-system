using CMS.Infrastructure;
using CMS.Services;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMS.Presentation
{
    /// <summary>
    /// Static store สำหรับ app-level state ของ UI layer
    /// </summary>
    public static class GlobalData
    {
        public static List<string>     CombinedList { get; set; }
        public static string           Token        { get; set; }
        public static List<MenuAction> MenuActions  { get; private set; }

        public static void SetMenuActions(List<MenuAction> menuActions)
        {
            MenuActions = menuActions;
        }
    }

    /// <summary>
    /// Presentation-layer coordinator สำหรับ Menu API
    /// — ไม่สืบทอดจาก SessionContext, ไม่ทำ HTTP call โดยตรง
    /// — delegate งาน HTTP ให้ IMenuApiService
    /// </summary>
    public class MenuControl
    {
        private readonly IMenuApiService _menuApiService;

        public MenuControl()
        {
            _menuApiService = new MenuApiService(new ConfigManager());
        }

        /// <summary>
        /// ดึง UserMenuModel จาก API แล้วประมวลผลลงใน GlobalData.MenuActions
        /// </summary>
        public async Task<UserMenuModel> Request(string accessToken)
        {
            UserMenuModel model = await _menuApiService.GetUserMenuAsync(accessToken).ConfigureAwait(false);

            if (model?.MenuList != null && model.MenuList.Count > 0)
                ProcessMenu(model);

            return model;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Menu processing — แปลง MenuList ที่ nested เป็น flat List<MenuAction>
        // ─────────────────────────────────────────────────────────────────────

        private static void ProcessMenu(UserMenuModel model)
        {
            var combinedList = new List<string>();
            CollectMenusAndActions(model.MenuList, combinedList);
            GlobalData.CombinedList = combinedList;

            var menuActions = new List<MenuAction>();
            foreach (string entry in combinedList)
            {
                string cleaned = entry.Replace("Action: ", "");
                string[] parts = cleaned.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                var action = new MenuAction();
                foreach (string part in parts)
                {
                    string[] kv = part.Split(new[] { ": " }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (kv.Length != 2) continue;
                    switch (kv[0])
                    {
                        case "MenuDesc":   action.MenuDesc   = kv[1]; break;
                        case "MenuCode":   action.MenuCode   = kv[1]; break;
                        case "ActionDesc": action.ActionDesc = kv[1]; break;
                        case "IsChecked":  action.IsChecked  = kv[1]; break;
                    }
                }
                menuActions.Add(action);
            }

            GlobalData.SetMenuActions(menuActions);
        }

        private static void CollectMenusAndActions(List<MenuItemDto> menus, List<string> result)
        {
            if (menus == null) return;
            foreach (var menu in menus)
            {
                if (menu.Actions != null)
                {
                    foreach (var action in menu.Actions)
                        result.Add(
                            $"Action: MenuDesc: {menu.MenuDesc}, MenuCode: {menu.MenuCode}, " +
                            $"ActionDesc: {action.MenuActionDesc}, IsChecked: {action.IsChecked} ");
                }

                if (menu.SubMenu != null && menu.SubMenu.Count > 0)
                    CollectMenusAndActions(menu.SubMenu, result);
            }
        }
    }
}
