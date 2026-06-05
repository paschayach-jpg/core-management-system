namespace CMS.Services.DTOs
{
    public class LdapUserResult
    {
        public string SamAccountName { get; set; }
        public string DisplayName    { get; set; }
        public string Mail           { get; set; }
        public string Department     { get; set; }
        public string Title          { get; set; }

        public string DropdownLabel
        {
            get
            {
                var dept = string.IsNullOrEmpty(Department) ? "" : " — " + Department;
                return string.Format("{0}{1} [{2}]", DisplayName, dept, SamAccountName);
            }
        }
    }
}
