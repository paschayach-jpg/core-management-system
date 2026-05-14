using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Presentation
{
    public class UserMenuModel
    {
        //root
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int RoleId { get; set; }
        public string RoleDesc { get; set; }
        public int ProgramId { get; set; }
        public string ProgramDesc { get; set; }
        public List<MenuModel> MenuList { get; set; }
    }
    public class MenuModel
    {
        //menu
        public int MenuId { get; set; }
        public string MenuDesc { get; set; }
        public int MenuTypeId { get; set; }
        public int? RefMenuId { get; set; }
        public string PathUrl { get; set; }
        public string MenuCode { get; set; }
        public MenuRedirectType menuRedirectType { get; set; }
        public List<MenuActionsList> Actions { get; set; }
        public List<MenuModel> SubMenu { get; set; }
    }
    public class MenuActionsList
    {
        //Actions
        public int MenuActionId { get; set; }
        public int MenuId { get; set; }
        public string MenuActionDesc { get; set; }
        public bool IsChecked { get; set; }
    }
    // คลาสสำหรับเก็บtype
    public class MenuRedirectType
    {
        public int id { get; set; }
        public string name { get; set; }
    }
       
    // คลาสสำหรับเก็บข้อมูลเมนู
    public class MenuAction
    {
        public string MenuDesc { get; set; }
        public string MenuCode { get; set; }
        public string ActionDesc { get; set; }
        public string IsChecked { get; set; }
        public string MenuType { get; set; }
    }

    public class EmployeeInfo
    {
        [JsonProperty("employee_Code")]
        public string EmployeeCode { get; set; }

        [JsonProperty("employee_Title_Name_En")]
        public string EmployeeTitleNameEn { get; set; }

        [JsonProperty("employee_First_Name_En")]
        public string EmployeeFirstNameEn { get; set; }

        [JsonProperty("employee_Last_Name_En")]
        public string EmployeeLastNameEn { get; set; }

        [JsonProperty("employee_Title_Name_Th")]
        public string EmployeeTitleNameTh { get; set; }

        [JsonProperty("employee_First_Name_Th")]
        public string EmployeeFirstNameTh { get; set; }

        [JsonProperty("employee_Last_Name_Th")]
        public string EmployeeLastNameTh { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("division_Code")]
        public string DivisionCode { get; set; }

        [JsonProperty("division_Name_En")]
        public string DivisionNameEn { get; set; }

        [JsonProperty("division_Name_Th")]
        public string DivisionNameTh { get; set; }

        [JsonProperty("department_Code")]
        public string DepartmentCode { get; set; }

        [JsonProperty("department_Name_En")]
        public string DepartmentNameEn { get; set; }

        [JsonProperty("department_Name_Th")]
        public string DepartmentNameTh { get; set; }
    }

}
