using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Presentation.Modules.Workflow.Presenters.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.Workflow.Presenters
{
    /// <summary>
    /// Presenter for Plugin Registry form (WFE-07).
    /// จัดการ CRUD บน s_WorkflowPlugin — Admin เท่านั้น
    /// SRS FR-008-22, FR-008-25, BR-008-11
    /// </summary>
    public class PluginRegistryPresenter
    {
        private readonly IPluginRegistryView     _view;
        private readonly IWorkflowPluginService  _pluginService;
        private readonly IAuditService           _auditService;
        private readonly ILoggingService         _loggingService;
        private readonly UserContext             _userContext;

        private List<WorkflowPluginResult> _currentPlugins = new List<WorkflowPluginResult>();

        public PluginRegistryPresenter(
            IPluginRegistryView    view,
            IWorkflowPluginService pluginService,
            IAuditService          auditService,
            ILoggingService        loggingService,
            UserContext            userContext)
        {
            _view           = view           ?? throw new ArgumentNullException("view");
            _pluginService  = pluginService  ?? throw new ArgumentNullException("pluginService");
            _auditService   = auditService   ?? throw new ArgumentNullException("auditService");
            _loggingService = loggingService ?? throw new ArgumentNullException("loggingService");
            _userContext    = userContext     ?? throw new ArgumentNullException("userContext");
        }

        // ── Load ─────────────────────────────────────────────────────────────

        /// <summary>โหลด Plugin ทั้งหมด พร้อม filter ปัจจุบัน</summary>
        public void LoadPlugins()
        {
            try
            {
                var filter = new WorkflowPluginFilter
                {
                    SearchKeyword = _view.GetSearchFilter(),
                    StatusFilter  = _view.GetStatusFilter(),
                    TenantId      = _userContext.TenantId
                };

                _currentPlugins = _pluginService.GetPlugins(filter) ?? new List<WorkflowPluginResult>();
                _view.DisplayPlugins(_currentPlugins);

                int active   = _currentPlugins.Count(p => p.IsActive);
                int inactive = _currentPlugins.Count - active;
                _view.UpdateSummaryCards(_currentPlugins.Count, active, inactive);

                _loggingService.Info(string.Format("PluginRegistry: loaded {0} plugins", _currentPlugins.Count));
            }
            catch (Exception ex)
            {
                _loggingService.Error("PluginRegistry LoadPlugins: " + ex.Message, ex);
                _view.ShowError("โหลดรายการ Plugin ไม่สำเร็จ: " + ex.Message);
            }
        }

        /// <summary>Search ด้วย filter ปัจจุบัน</summary>
        public void Search() => LoadPlugins();

        // ── Register ─────────────────────────────────────────────────────────

        /// <summary>
        /// ลงทะเบียน Plugin ใหม่ใน s_WorkflowPlugin
        /// BR-008-11: Plugin ต้อง register ก่อน Definition Activate ได้
        /// </summary>
        public void RegisterPlugin(RegisterPluginRequest request)
        {
            if (request == null) { _view.ShowError("ข้อมูล Plugin ไม่ถูกต้อง"); return; }
            if (string.IsNullOrWhiteSpace(request.PluginCode))  { _view.ShowError("กรุณาระบุ Plugin Code"); return; }
            if (string.IsNullOrWhiteSpace(request.PluginName))  { _view.ShowError("กรุณาระบุ Plugin Name"); return; }
            if (string.IsNullOrWhiteSpace(request.ClassName))   { _view.ShowError("กรุณาระบุ Class Name"); return; }

            try
            {
                request.RegisteredBy = _userContext.ADUserName;
                request.TenantId     = _userContext.TenantId;

                _pluginService.RegisterPlugin(request);

                _auditService.Log(new AuditEntry
                {
                    Action    = "WorkflowPluginRegistered",
                    TableName = "s_WorkflowPlugin",
                    RecordID  = 0,
                    UserName  = _userContext.ADUserName,
                    NewValue  = string.Format("Registered plugin: {0} ({1})", request.PluginCode, request.ClassName),
                    AuditDate = DateTime.Now
                });

                _loggingService.Info(string.Format("Plugin registered: {0} by {1}", request.PluginCode, _userContext.ADUserName));
                _view.ShowInfo(string.Format("ลงทะเบียน Plugin '{0}' เรียบร้อยแล้ว", request.PluginCode));
                LoadPlugins();
            }
            catch (Exception ex)
            {
                _loggingService.Error("RegisterPlugin: " + ex.Message, ex);
                _view.ShowError("ลงทะเบียนไม่สำเร็จ: " + ex.Message);
            }
        }

        // ── Enable / Disable ─────────────────────────────────────────────────

        /// <summary>เปิดใช้งาน Plugin ที่เลือก</summary>
        public void EnablePlugin(WorkflowPluginResult plugin)
        {
            if (plugin == null) { _view.ShowError("กรุณาเลือก Plugin ที่ต้องการเปิดใช้งาน"); return; }

            try
            {
                _pluginService.SetPluginStatus(plugin.PluginId, true, _userContext.TenantId);

                _auditService.Log(new AuditEntry
                {
                    Action    = "WorkflowPluginEnabled",
                    TableName = "s_WorkflowPlugin",
                    RecordID  = plugin.PluginId,
                    UserName  = _userContext.ADUserName,
                    NewValue  = string.Format("Plugin enabled: {0}", plugin.PluginCode),
                    AuditDate = DateTime.Now
                });

                _loggingService.Info(string.Format("Plugin {0} enabled by {1}", plugin.PluginCode, _userContext.ADUserName));
                _view.ShowInfo(string.Format("เปิดใช้งาน Plugin '{0}' เรียบร้อยแล้ว", plugin.PluginCode));
                LoadPlugins();
            }
            catch (Exception ex)
            {
                _loggingService.Error("EnablePlugin: " + ex.Message, ex);
                _view.ShowError("เปิดใช้งานไม่สำเร็จ: " + ex.Message);
            }
        }

        /// <summary>
        /// ปิดใช้งาน Plugin ที่เลือก
        /// หมายเหตุ: Plugin ที่ disable ยังคง run ได้บน Instance ที่ Active อยู่ (FrozenDefJSON)
        /// Definition ใหม่จะ Activate ไม่ได้หาก Plugin ยัง Inactive
        /// </summary>
        public void DisablePlugin(WorkflowPluginResult plugin)
        {
            if (plugin == null) { _view.ShowError("กรุณาเลือก Plugin ที่ต้องการปิดใช้งาน"); return; }
            if (plugin.IsBuiltIn) { _view.ShowError(string.Format("ไม่สามารถปิดใช้งาน Built-in Plugin '{0}' ได้", plugin.PluginCode)); return; }

            try
            {
                _pluginService.SetPluginStatus(plugin.PluginId, false, _userContext.TenantId);

                _auditService.Log(new AuditEntry
                {
                    Action    = "WorkflowPluginDisabled",
                    TableName = "s_WorkflowPlugin",
                    RecordID  = plugin.PluginId,
                    UserName  = _userContext.ADUserName,
                    NewValue  = string.Format("Plugin disabled: {0}", plugin.PluginCode),
                    AuditDate = DateTime.Now
                });

                _loggingService.Info(string.Format("Plugin {0} disabled by {1}", plugin.PluginCode, _userContext.ADUserName));
                _view.ShowInfo(string.Format("ปิดใช้งาน Plugin '{0}' เรียบร้อยแล้ว", plugin.PluginCode));
                LoadPlugins();
            }
            catch (Exception ex)
            {
                _loggingService.Error("DisablePlugin: " + ex.Message, ex);
                _view.ShowError("ปิดใช้งานไม่สำเร็จ: " + ex.Message);
            }
        }

        // ── Delete ───────────────────────────────────────────────────────────

        /// <summary>
        /// ลบ Plugin (เฉพาะ Custom Plugin ที่ไม่ถูกใช้ใน Definition ใดๆ)
        /// Built-in Plugin ลบไม่ได้
        /// </summary>
        public void DeletePlugin(WorkflowPluginResult plugin)
        {
            if (plugin == null) { _view.ShowError("กรุณาเลือก Plugin ที่ต้องการลบ"); return; }
            if (plugin.IsBuiltIn) { _view.ShowError(string.Format("ไม่สามารถลบ Built-in Plugin '{0}' ได้", plugin.PluginCode)); return; }

            try
            {
                _pluginService.DeletePlugin(plugin.PluginId, _userContext.TenantId);

                _auditService.Log(new AuditEntry
                {
                    Action    = "WorkflowPluginDeleted",
                    TableName = "s_WorkflowPlugin",
                    RecordID  = plugin.PluginId,
                    UserName  = _userContext.ADUserName,
                    NewValue  = string.Format("Plugin deleted: {0}", plugin.PluginCode),
                    AuditDate = DateTime.Now
                });

                _loggingService.Info(string.Format("Plugin {0} deleted by {1}", plugin.PluginCode, _userContext.ADUserName));
                _view.ShowInfo(string.Format("ลบ Plugin '{0}' เรียบร้อยแล้ว", plugin.PluginCode));
                LoadPlugins();
            }
            catch (InvalidOperationException ioEx)
            {
                // Plugin ถูกใช้งานอยู่ใน Definition
                _view.ShowError(ioEx.Message);
            }
            catch (Exception ex)
            {
                _loggingService.Error("DeletePlugin: " + ex.Message, ex);
                _view.ShowError("ลบ Plugin ไม่สำเร็จ: " + ex.Message);
            }
        }
    }
}
