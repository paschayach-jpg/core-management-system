using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMS.Presentation
{
    /// <summary>
    /// Presentation-layer coordinator สำหรับ Menu
    /// รับข้อมูลจาก IMenuController (CMS.Controllers) เท่านั้น
    /// ไม่อ้างอิง CMS.Services หรือ CMS.Infrastructure โดยตรง
    /// </summary>
    public class MenuControl
    {
        private readonly IMenuController _menuController;

        public MenuControl(IMenuController menuController)
        {
            _menuController = menuController ?? throw new ArgumentNullException("menuController");
        }

        /// <summary>
        /// ดึง UserMenuModel แล้วประมวลผลลงใน GlobalData.MenuActions
        /// </summary>
        public async Task<UserMenuModel> Request(string accessToken)
        {
            UserMenuModel model = await _menuController.GetUserMenuAsync(accessToken).ConfigureAwait(false);

            if (model?.MenuList != null && model.MenuList.Count > 0)
                ProcessMenu(model);

            return model;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Menu processing — แปลง MenuList nested → flat List<MenuAction>
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
