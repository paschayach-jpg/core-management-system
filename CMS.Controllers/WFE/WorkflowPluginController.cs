using System;
using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers
{
    /// <summary>
    /// Controller สำหรับ Plugin Registry (WFE-07)
    /// thin wrapper รอบ IWorkflowPluginService — Admin เท่านั้น
    /// SRS FR-008-22, FR-008-25, UC-008-12, BR-008-11
    /// </summary>
    public class WorkflowPluginController
    {
        private readonly IWorkflowPluginService _pluginService;
        private readonly ILoggingService        _loggingService;

        public WorkflowPluginController(
            IWorkflowPluginService pluginService   = null,
            ILoggingService        loggingService  = null)
        {
            _pluginService  = pluginService;
            _loggingService = loggingService;
        }

        /// <summary>ดึงรายการ Plugin ทั้งหมดตาม filter</summary>
        public List<WorkflowPluginResult> GetPlugins(WorkflowPluginFilter filter)
        {
            try
            {
                if (filter == null)
                    throw new ArgumentNullException("filter");

                _loggingService?.Info("WorkflowPluginController.GetPlugins");
                return _pluginService.GetPlugins(filter);
            }
            catch (Exception ex)
            {
                _loggingService?.Error("GetPlugins: " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>ดึง Plugin เดี่ยวตาม PluginCode</summary>
        public WorkflowPluginResult GetPlugin(string pluginCode, int tenantId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pluginCode))
                    throw new ArgumentException("pluginCode is required");

                _loggingService?.Info("WorkflowPluginController.GetPlugin: " + pluginCode);
                return _pluginService.GetPlugin(pluginCode, tenantId);
            }
            catch (Exception ex)
            {
                _loggingService?.Error("GetPlugin: " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// ลงทะเบียน Plugin ใหม่ใน s_WorkflowPlugin
        /// BR-008-11: Plugin ต้อง register ก่อน Definition Activate ได้
        /// </summary>
        public void RegisterPlugin(RegisterPluginRequest request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException("request");

                if (string.IsNullOrWhiteSpace(request.PluginCode))
                    throw new ArgumentException("PluginCode is required");

                if (string.IsNullOrWhiteSpace(request.ClassName))
                    throw new ArgumentException("ClassName is required");

                _loggingService?.Info("WorkflowPluginController.RegisterPlugin: " + request.PluginCode);
                _pluginService.RegisterPlugin(request);
            }
            catch (Exception ex)
            {
                _loggingService?.Error("RegisterPlugin: " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>เปิด/ปิดใช้งาน Plugin (IsActive flag)</summary>
        public void SetPluginStatus(int pluginId, bool isActive, int tenantId)
        {
            try
            {
                if (pluginId <= 0)
                    throw new ArgumentException("Invalid pluginId");

                _loggingService?.Info(string.Format("WorkflowPluginController.SetPluginStatus: id={0} active={1}", pluginId, isActive));
                _pluginService.SetPluginStatus(pluginId, isActive, tenantId);
            }
            catch (Exception ex)
            {
                _loggingService?.Error("SetPluginStatus: " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// ลบ Custom Plugin (Built-in ลบไม่ได้)
        /// ถ้า Plugin ถูกอ้างอิงใน Definition ที่ Active อยู่ → InvalidOperationException
        /// </summary>
        public void DeletePlugin(int pluginId, int tenantId)
        {
            try
            {
                if (pluginId <= 0)
                    throw new ArgumentException("Invalid pluginId");

                _loggingService?.Info("WorkflowPluginController.DeletePlugin: id=" + pluginId);
                _pluginService.DeletePlugin(pluginId, tenantId);
            }
            catch (InvalidOperationException)
            {
                // Plugin ถูกใช้งานอยู่ — ปล่อยให้ caller จัดการ
                throw;
            }
            catch (Exception ex)
            {
                _loggingService?.Error("DeletePlugin: " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>ตรวจสอบว่า PluginCode มีอยู่และ Active ใน Registry หรือไม่
        /// ใช้โดย DefinitionValidator ก่อน Activate (BR-008-14, FR-008-07)</summary>
        public bool IsPluginRegisteredAndActive(string pluginCode, int tenantId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pluginCode)) return false;

                var plugin = _pluginService.GetPlugin(pluginCode, tenantId);
                return plugin != null && plugin.IsActive;
            }
            catch (Exception ex)
            {
                _loggingService?.Error("IsPluginRegisteredAndActive: " + ex.Message, ex);
                return false;
            }
        }
    }
}
