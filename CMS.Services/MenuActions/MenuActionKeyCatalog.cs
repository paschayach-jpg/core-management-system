using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace CMS.Services
{
    public static class MenuActionKeyCatalog
    {
        private static readonly string[] BuiltInActionKeys =
        {
            "Admin",
            "Home",
            "ShareDialog",
            "SharedWithMe",
            "ShareManagement",
            "DocumentGroup",
            "OrgGroup",
            "PermissionGroup",
            "ReportGroup",
            "SystemGroup",
            "OrgStructure",
            "UserManagement",
            "UserAccess",
            "PermissionAdmin",
            "FolderSettings",
            "UserPermissionSummary",
            "ApprovalInbox",
            "ReportExcel",
            "PdpaConsent",
            "PdpaReport",
            "AccessLog",
            "AuditLog",
            "MenuManagement",
            "ConfigAdmin",
            "AuthorityConfig",
            "UserPermissionDiagnostic",
            "AuthorityByOrgReport",
            "DownloadApproverConfig",
            "DownloadUserPermission",
            "MyDownloadRequests",
            "FolderTemplate",
            "ArchiveFolder",
            "CopyStructure",
            "ChangeLevel",
            "SyncEmployee",
            "ImportAdUsers",
            "RecycleBin",
            "KnowledgeBaseForm",
            "KBMainForm",
            "KBCategoryForm",
            "WorkflowDashboard",
            "WorkflowDetail",
            "WorkflowTemplateList",
            "WorkflowDesigner",
            "WorkflowDelegation",
            "SLAReport",
            "WorkflowVisualFlow",
            "QuotaConfig",
            "QuotaRequest",
            "QuotaApproval",
            "FolderUsageReport",
            "ProjectMain",
            "Frm_Project_List",
            "OrgMasterManagement"
        };

        public static string[] GetAvailableActionKeys()
        {
            var keys = new List<string>(BuiltInActionKeys);
            keys.AddRange(ParseListSetting("MenuActionKeys"));
            keys.AddRange(ParseSettingsByPrefix("MenuActionKeys."));

            return keys
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToArray();
        }

        public static string ResolveConfiguredActionKey(string configuredValue)
        {
            if (string.IsNullOrWhiteSpace(configuredValue))
                return null;

            string normalized = NormalizeToken(configuredValue);
            if (string.IsNullOrEmpty(normalized))
                return null;

            if (string.Equals(normalized, "workflowdashboardformnew", StringComparison.OrdinalIgnoreCase))
                return "WorkflowDashboard";
            if (string.Equals(normalized, "workflowdetailformnew", StringComparison.OrdinalIgnoreCase))
                return "WorkflowDetail";
            if (string.Equals(normalized, "workflowtemplatelistform", StringComparison.OrdinalIgnoreCase))
                return "WorkflowTemplateList";
            if (string.Equals(normalized, "workflowdesignerform", StringComparison.OrdinalIgnoreCase))
                return "WorkflowDesigner";
            if (string.Equals(normalized, "workflowdelegationform", StringComparison.OrdinalIgnoreCase))
                return "WorkflowDelegation";
            if (string.Equals(normalized, "slareportform", StringComparison.OrdinalIgnoreCase))
                return "SLAReport";
            if (string.Equals(normalized, "workflowvisualflowform", StringComparison.OrdinalIgnoreCase))
                return "WorkflowVisualFlow";
            if (string.Equals(normalized, "folderquotaconfigform", StringComparison.OrdinalIgnoreCase))
                return "QuotaConfig";
            if (string.Equals(normalized, "folderquotarequestform", StringComparison.OrdinalIgnoreCase))
                return "QuotaRequest";
            if (string.Equals(normalized, "folderquotaapprovalform", StringComparison.OrdinalIgnoreCase))
                return "QuotaApproval";
            if (string.Equals(normalized, "folderusagereportform", StringComparison.OrdinalIgnoreCase))
                return "FolderUsageReport";
            if (string.Equals(normalized, "orgmastermanagementform", StringComparison.OrdinalIgnoreCase))
                return "OrgMasterManagement";

            foreach (var builtIn in BuiltInActionKeys)
            {
                if (NormalizeToken(builtIn) == normalized)
                    return builtIn;
            }

            foreach (var configured in ParseListSetting("MenuActionKeys"))
            {
                if (NormalizeToken(configured) == normalized)
                    return configured.Trim();
            }

            foreach (var configured in ParseSettingsByPrefix("MenuActionKeys."))
            {
                if (NormalizeToken(configured) == normalized)
                    return configured.Trim();
            }

            string aliasTarget;
            if (GetAliasMappings().TryGetValue(normalized, out aliasTarget))
                return aliasTarget;

            return null;
        }

        private static IEnumerable<string> ParseListSetting(string key)
        {
            string value = SafeAppSetting(key);
            if (string.IsNullOrWhiteSpace(value))
                return Enumerable.Empty<string>();

            return value
                .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => item.Length > 0);
        }

        private static Dictionary<string, string> GetAliasMappings()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            AddAliasMappings(result, SafeAppSetting("MenuActionKeyAliases"));

            foreach (var raw in GetSettingsByPrefix("MenuActionKeyAliases."))
                AddAliasMappings(result, raw);

            return result;
        }

        private static void AddAliasMappings(Dictionary<string, string> result, string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return;

            foreach (var item in raw.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = item.Split(new[] { '=' }, 2);
                if (parts.Length != 2)
                    continue;

                string alias = NormalizeToken(parts[0]);
                string target = ResolveBuiltInOrConfigured(parts[1]);
                if (!string.IsNullOrWhiteSpace(alias) && !string.IsNullOrWhiteSpace(target))
                    result[alias] = target;
            }
        }

        private static IEnumerable<string> ParseSettingsByPrefix(string prefix)
        {
            return GetSettingsByPrefix(prefix)
                .SelectMany(value => value.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(item => item.Trim())
                .Where(item => item.Length > 0);
        }

        private static IEnumerable<string> GetSettingsByPrefix(string prefix)
        {
            try
            {
                return ConfigurationManager.AppSettings.AllKeys
                    .Where(key => !string.IsNullOrWhiteSpace(key) && key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Select(key => ConfigurationManager.AppSettings[key])
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        private static string ResolveBuiltInOrConfigured(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string resolved = ResolveConfiguredActionKey(value);
            return string.IsNullOrWhiteSpace(resolved) ? value.Trim() : resolved;
        }

        private static string SafeAppSetting(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string token = value.Trim();

            if (token.StartsWith("#sym:", StringComparison.OrdinalIgnoreCase))
                token = token.Substring(5);
            else if (token.StartsWith("sym:", StringComparison.OrdinalIgnoreCase))
                token = token.Substring(4);

            int lastColon = token.LastIndexOf(':');
            if (lastColon >= 0 && lastColon < token.Length - 1)
                token = token.Substring(lastColon + 1);

            int lastDot = token.LastIndexOf('.');
            if (lastDot >= 0 && lastDot < token.Length - 1)
                token = token.Substring(lastDot + 1);

            return token.Replace("_", string.Empty)
                        .Replace("-", string.Empty)
                        .Replace(" ", string.Empty)
                        .ToLowerInvariant();
        }
    }
}
