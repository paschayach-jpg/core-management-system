using System;
using System.Linq;
using CMS.Domain.Models;

namespace CMS.Presentation.Helpers
{
    /// <summary>
    /// Helper สำหรับแปลงการแสดงผลชื่อผู้ใช้และการค้นหาข้อมูลผู้ใช้ในระบบ Presentation
    /// </summary>
    public static class UserDisplayHelper
    {
        /// <summary>
        /// คืนค่าชื่อผู้ใช้สำหรับแสดงผล โดยตัด prefix องค์กร (เช่น LAW/BUI/)
        /// ลำดับ: DisplayName (ตัด prefix ก่อน / ตัวสุดท้าย, Trim) → FullNameTh → FullNameEn → ADUserName → "User " + UserID
        /// </summary>
        public static string NameOf(UserAccount u)
        {
            if (u == null) return string.Empty;

            if (!string.IsNullOrWhiteSpace(u.DisplayName))
            {
                string name = u.DisplayName.Trim();
                int idx = name.LastIndexOf('/');
                if (idx >= 0 && idx < name.Length - 1)
                    name = name.Substring(idx + 1).Trim();

                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }

            if (!string.IsNullOrWhiteSpace(u.FullNameTh))
                return u.FullNameTh.Trim();

            if (!string.IsNullOrWhiteSpace(u.FullNameEn))
                return u.FullNameEn.Trim();

            if (!string.IsNullOrWhiteSpace(u.ADUserName))
                return u.ADUserName.Trim();

            return "User " + u.UserID;
        }

        /// <summary>
        /// ตรวจสอบว่าผู้ใช้ตรงกับคำค้นหาหรือไม่
        /// ค้นหาใน: DisplayName, ADUserName, EmployeeCode, ชื่อ-นามสกุล (TH/EN), Email
        /// รองรับการค้นหาหลายคำแยกตามช่องว่าง (ทุกคำต้อง match)
        /// </summary>
        public static bool Matches(UserAccount u, string query)
        {
            if (u == null) return false;
            if (string.IsNullOrWhiteSpace(query)) return true;

            string[] terms = query.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return terms.All(term =>
                   Contains(u.DisplayName, term)
                || Contains(u.ADUserName, term)
                || Contains(u.EmployeeCode, term)
                || Contains(u.EmployeeFirstNameTh, term)
                || Contains(u.EmployeeLastNameTh, term)
                || Contains(u.FullNameTh, term)
                || Contains(u.EmployeeFirstNameEn, term)
                || Contains(u.EmployeeLastNameEn, term)
                || Contains(u.FullNameEn, term)
                || Contains(u.Email, term));
        }

        private static bool Contains(string value, string query)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
