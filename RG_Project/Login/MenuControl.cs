
using RG_Project.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RG_Project.Login
{
    // StaticClass.cs
    public static class GlobalData
    {
        public static List<string> CombinedList { get; set; }
        public static string Token { get; set; }
        public static List<MenuAction> MenuActions { get; private set; }
        public static void SetMenuActions(List<MenuAction> menuActions)
        {
            MenuActions = menuActions;
        }
    }
    public class MenuControl : LoginManager
    {
        //private string _apiUrl = "http://150.1.8.28:9090/api/UserInfoRequest";
        public string munu {  get; set; }

        public class LoginRequest
        {
          
            public string UserToken { get; set; }

            public string ProgramId { get; set; }
        }
        public string MenuUrl()
        {
            string MenuControl = null;
            string _MenuConfig = ConfigurationManager.AppSettings["MenuControl"];
            switch (_MenuConfig)
            {
                case "dev":
                    MenuControl = "http://150.1.8.29:9080/api/UserInfoRequest";
                    break;
                case "sit":
                    MenuControl = "http://150.1.8.29:9070/api/UserInfoRequest";
                    break;
                case "uat":
                    MenuControl = "http://150.1.8.29:9070/api/UserInfoRequest";
                    break;
                case "prd":
                    MenuControl = "http://150.1.8.28:9090/api/UserInfoRequest";
                    break;
                    //default:
                    //    throw new ArgumentException($"Invalid environment: {_config.DBServer.Server}");
            }
            return MenuControl;
        }
        public async Task<UserMenuModel> Request(int type)
        {
            string _apiUrl = MenuUrl();
            UserMenuModel userMenuModel = new UserMenuModel();
            LoginRequest data;
            if (type == 1)
            {
                data = new LoginRequest
                {
                    UserToken = GlobalData.Token,//LoginManager.Instance.Token,
                    ProgramId = "6"
                };
            }
            else
            {
                data = new LoginRequest
                {
                    UserToken = LoginManager.Instance.Token,
                    ProgramId = "6"
                };
            }
            //var data = new LoginRequest
            //{
            //    UserToken = GlobalData.Token,//LoginManager.Instance.Token,
            //    ProgramId = "6"
            //};
            string json = JsonConvert.SerializeObject(data);
            using (HttpClient client = new HttpClient()) 
            {
                var  content = new StringContent(json, Encoding.UTF8, "application/json");
               
                HttpResponseMessage response = await client.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    userMenuModel = JsonConvert.DeserializeObject<UserMenuModel>(responseContent);
                    if (userMenuModel.MenuList != null &&userMenuModel.MenuList.Count > 0) 
                    {
                        controlMenu(userMenuModel);
                    }
                }
                else { return null; }
               
            
            }
            return userMenuModel;
        }

        public void controlMenu(UserMenuModel userMenuModel)
        {
            // สร้างรายการเก็บข้อมูลทั้งหมด
            List<string> combinedList = new List<string>();
            CollectMenusAndActions(userMenuModel.MenuList, combinedList);

            // เก็บ combinedList ใน Static Class
            GlobalData.CombinedList = combinedList;

            // สร้าง List สำหรับเก็บ MenuActions
            List<MenuAction> MenuActions = new List<MenuAction>();

            foreach (var input in combinedList)
            {
                // ลบ "Action: " ออก
                string cleanedInput = input.Replace("Action: ", "");

                // แยกข้อมูลด้วย ", "
                string[] parts = cleanedInput.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                // สร้างออบเจ็กต์ใหม่
                MenuAction action = new MenuAction();

                foreach (var part in parts)
                {
                    // แยกฟิลด์และค่าด้วย ": "
                    string[] keyValue = part.Split(new[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                    if (keyValue.Length == 2)
                    {
                        switch (keyValue[0])
                        {
                            case "MenuDesc":
                                action.MenuDesc = keyValue[1];
                                break;
                            case "MenuCode":
                                action.MenuCode = keyValue[1];
                                break;
                            
                            case "ActionDesc":
                                action.ActionDesc = keyValue[1];
                                break;
                            case "IsChecked":
                                action.IsChecked = keyValue[1];
                                break;
                       
                        }
                    }
                }
                // เพิ่มออบเจ็กต์เข้า List
                MenuActions.Add(action);
            }
            // ตั้งค่า MenuActions ใน GlobalData
            GlobalData.SetMenuActions(MenuActions);

        }
        /// <summary>
        /// ฟังก์ชันแบบ Recursive เพื่อรวมข้อมูลเมนูและ Actions
        /// </summary>
        static void CollectMenusAndActions(List<MenuModel> menus, List<string> combinedList)
        {
            foreach (var menu in menus)
            {
                // เพิ่มข้อมูล Actions ของเมนูในรายการ
                if ((menu.Actions != null && menu.Actions.Count > 0) || menu.SubMenu.Count > 0)
                {
                    foreach (var action in menu.Actions)
                    {
                        combinedList.Add($"Action: MenuDesc: {menu.MenuDesc}, MenuCode: {menu.MenuCode}, ActionDesc: {action.MenuActionDesc}, IsChecked: {action.IsChecked} ");
                    }

                    // ตรวจสอบ SubMenu
                    if (menu.SubMenu != null && menu.SubMenu.Count > 0)
                    {
                        CollectMenusAndActions(menu.SubMenu, combinedList);
                    }
                }
            }
        }

    }

}
