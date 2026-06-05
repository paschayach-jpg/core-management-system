using System.Collections.Generic;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow.Presenters.Interfaces
{
    /// <summary>
    /// View interface for Plugin Registry form (WFE-07).
    /// Admin จัดการ s_WorkflowPlugin — register, enable/disable Plugin ที่ใช้กับ AutoTask.
    /// SRS FR-008-25, UC-008-12, BR-008-11
    /// </summary>
    public interface IPluginRegistryView
    {
        /// <summary>แสดงรายการ Plugin ทั้งหมดในระบบ</summary>
        void DisplayPlugins(List<WorkflowPluginResult> plugins);

        /// <summary>เปิด/ปิดปุ่ม action ตามการเลือก</summary>
        void SetActionButtonsEnabled(bool canEnable, bool canDisable, bool canDelete, bool canDetail);

        /// <summary>อัปเดต summary count บน Cards</summary>
        void UpdateSummaryCards(int totalPlugins, int activePlugins, int inactivePlugins);

        /// <summary>แสดงข้อความ error</summary>
        void ShowError(string message);

        /// <summary>แสดงข้อความ info</summary>
        void ShowInfo(string message);

        /// <summary>รับค่า keyword จาก search box</summary>
        string GetSearchFilter();

        /// <summary>รับค่า status filter (null = ทั้งหมด, "Active", "Inactive")</summary>
        string GetStatusFilter();
    }
}
