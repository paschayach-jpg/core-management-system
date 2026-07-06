using System.Runtime.Serialization;

namespace CMS.Services.DTOs
{
    /// <summary>
    /// Response จาก AD API endpoint /login
    /// </summary>
    [DataContract]
    public class LoginResponse
    {
        [DataMember(Name = "accessToken")]
        public string AccessToken { get; set; }

        [DataMember(Name = "token")]
        public string Token { get; set; }

        [DataMember(Name = "refreshToken")]
        public string RefreshToken { get; set; }

        [DataMember(Name = "refreshTokenExpired")]
        public string RefreshTokenExpired { get; set; }

        [DataMember(Name = "employee_Info")]
        public EmployeeInfo EmployeeInfo { get; set; }
    }

    /// <summary>
    /// ข้อมูลพนักงานที่ได้รับจาก AD API หลัง login สำเร็จ
    /// </summary>
    [DataContract]
    public class EmployeeInfo
    {
        [DataMember(Name = "employee_Code")]
        public string EmployeeCode { get; set; }

        [DataMember(Name = "employee_Title_Name_En")]
        public string EmployeeTitleNameEn { get; set; }

        [DataMember(Name = "employee_First_Name_En")]
        public string EmployeeFirstNameEn { get; set; }

        [DataMember(Name = "employee_Last_Name_En")]
        public string EmployeeLastNameEn { get; set; }

        [DataMember(Name = "employee_Title_Name_Th")]
        public string EmployeeTitleNameTh { get; set; }

        [DataMember(Name = "employee_First_Name_Th")]
        public string EmployeeFirstNameTh { get; set; }

        [DataMember(Name = "employee_Last_Name_Th")]
        public string EmployeeLastNameTh { get; set; }

        [DataMember(Name = "position")]
        public string Position { get; set; }

        [DataMember(Name = "division_Code")]
        public string DivisionCode { get; set; }

        [DataMember(Name = "division_Name_En")]
        public string DivisionNameEn { get; set; }

        [DataMember(Name = "division_Name_Th")]
        public string DivisionNameTh { get; set; }

        [DataMember(Name = "department_Code")]
        public string DepartmentCode { get; set; }

        [DataMember(Name = "department_Name_En")]
        public string DepartmentNameEn { get; set; }

        [DataMember(Name = "department_Name_Th")]
        public string DepartmentNameTh { get; set; }

        /// <summary>ชื่อเต็มภาษาไทย (รวม prefix + first + last)</summary>
        public string FullNameTh
        {
            get
            {
                return string.Format("{0}{1} {2}",
                    EmployeeTitleNameTh ?? "",
                    EmployeeFirstNameTh ?? "",
                    EmployeeLastNameTh  ?? "").Trim();
            }
        }

        /// <summary>ชื่อเต็มภาษาอังกฤษ (รวม prefix + first + last)</summary>
        public string FullNameEn
        {
            get
            {
                return string.Format("{0}{1} {2}",
                    EmployeeTitleNameEn ?? "",
                    EmployeeFirstNameEn ?? "",
                    EmployeeLastNameEn  ?? "").Trim();
            }
        }
    }
}
