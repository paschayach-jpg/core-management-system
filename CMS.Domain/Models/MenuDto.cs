using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMS.Domain.Models
{
    [DataContract]
    public class UserMenuModel
    {
        [DataMember(Name = "userId")]      public int    UserId      { get; set; }
        [DataMember(Name = "userName")]    public string UserName    { get; set; }
        [DataMember(Name = "roleId")]      public int    RoleId      { get; set; }
        [DataMember(Name = "roleDesc")]    public string RoleDesc    { get; set; }
        [DataMember(Name = "programId")]   public int    ProgramId   { get; set; }
        [DataMember(Name = "programDesc")] public string ProgramDesc { get; set; }
        [DataMember(Name = "menuList")]    public List<MenuItemDto> MenuList { get; set; }
    }

    [DataContract]
    public class MenuItemDto
    {
        [DataMember(Name = "menuId")]           public int    MenuId     { get; set; }
        [DataMember(Name = "menuDesc")]         public string MenuDesc   { get; set; }
        [DataMember(Name = "menuTypeId")]       public int    MenuTypeId { get; set; }
        [DataMember(Name = "refMenuId")]        public int?   RefMenuId  { get; set; }
        [DataMember(Name = "pathUrl")]          public string PathUrl    { get; set; }
        [DataMember(Name = "menuCode")]         public string MenuCode   { get; set; }
        [DataMember(Name = "menuRedirectType")] public MenuRedirectTypeDto MenuRedirectType { get; set; }
        [DataMember(Name = "actions")]          public List<MenuActionItemDto> Actions { get; set; }
        [DataMember(Name = "subMenu")]          public List<MenuItemDto>      SubMenu { get; set; }
    }

    [DataContract]
    public class MenuActionItemDto
    {
        [DataMember(Name = "menuActionId")]   public int    MenuActionId   { get; set; }
        [DataMember(Name = "menuId")]         public int    MenuId         { get; set; }
        [DataMember(Name = "menuActionDesc")] public string MenuActionDesc { get; set; }
        [DataMember(Name = "isChecked")]      public bool   IsChecked      { get; set; }
    }

    [DataContract]
    public class MenuRedirectTypeDto
    {
        [DataMember(Name = "id")]   public int    Id   { get; set; }
        [DataMember(Name = "name")] public string Name { get; set; }
    }
}
