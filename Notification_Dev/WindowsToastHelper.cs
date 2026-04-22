// =============================================
// E-Filing Notification System
// Services/WindowsToastHelper.cs
// Windows Toast & Balloon Notifications
// Compatible with .NET Framework 4.5
// ไม่ต้องติดตั้ง NuGet Package เพิ่มเติม
// =============================================

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using E_Filing.Notification.Models;

namespace E_Filing.Notification.Services
{
    /// <summary>
    /// Windows Notification Helper for .NET Framework 4.5
    /// ใช้ NotifyIcon สำหรับ Balloon Tip (Windows 7/8/10/11)
    /// รองรับ Windows Toast ผ่าน PowerShell (Windows 10/11)
    /// </summary>
    public class WindowsToastHelper : IDisposable
    {
        #region Singleton

        private static WindowsToastHelper _instance;
        private static readonly object _lock = new object();

        public static WindowsToastHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new WindowsToastHelper();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Fields

        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private bool _isInitialized;
        private bool _disposed;
        private string _appName;
        private NotificationDto _currentNotification;
        private bool _isWindows10OrLater;

        #endregion

        #region Events

        /// <summary>
        /// เมื่อคลิกที่ Notification
        /// </summary>
        public event EventHandler<NotificationDto> NotificationClicked;

        /// <summary>
        /// เมื่อ Double-click ที่ Tray Icon
        /// </summary>
        public event EventHandler TrayIconDoubleClicked;

        /// <summary>
        /// เมื่อคลิก "เปิด" ใน Context Menu
        /// </summary>
        public event EventHandler OpenMenuClicked;

        /// <summary>
        /// เมื่อคลิก "ออก" ใน Context Menu
        /// </summary>
        public event EventHandler ExitMenuClicked;

        #endregion

        #region Properties

        public bool IsInitialized { get { return _isInitialized; } }
        public bool IsWindows10OrLater { get { return _isWindows10OrLater; } }
        public bool UseToastNotification { get; set; }

        #endregion

        #region Constructor

        private WindowsToastHelper()
        {
            _isWindows10OrLater = CheckIsWindows10OrLater();
            UseToastNotification = _isWindows10OrLater;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize notification system
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="icon">Tray icon (null = use default)</param>
        public void Initialize(string appName = "E-Filing", Icon icon = null)
        {
            if (_isInitialized) return;

            _appName = appName ?? "E-Filing";

            try
            {
                // Create context menu
                _contextMenu = new ContextMenuStrip();
                _contextMenu.Items.Add("เปิด " + _appName, null, OnOpenMenuClick);
                _contextMenu.Items.Add("-");
                _contextMenu.Items.Add("ปิดการแจ้งเตือน", null, OnDisableNotificationClick);
                _contextMenu.Items.Add("-");
                _contextMenu.Items.Add("ออกจากระบบ", null, OnExitMenuClick);

                // Create notify icon
                _notifyIcon = new NotifyIcon();
                _notifyIcon.Icon = icon ?? GetDefaultIcon();
                _notifyIcon.Text = _appName + " Notification";
                _notifyIcon.ContextMenuStrip = _contextMenu;
                _notifyIcon.Visible = true;

                // Events
                _notifyIcon.DoubleClick += OnTrayIconDoubleClick;
                _notifyIcon.BalloonTipClicked += OnBalloonTipClicked;
                _notifyIcon.BalloonTipClosed += OnBalloonTipClosed;

                _isInitialized = true;
                Log("Initialized successfully (Windows 10+: " + _isWindows10OrLater + ")");
            }
            catch (Exception ex)
            {
                Log("Initialization failed: " + ex.Message);
            }
        }

        private bool CheckIsWindows10OrLater()
        {
            try
            {
                var version = Environment.OSVersion.Version;
                // Windows 10 = 10.0, Windows 8.1 = 6.3, Windows 8 = 6.2, Windows 7 = 6.1
                return version.Major >= 10 || (version.Major == 6 && version.Minor >= 2);
            }
            catch
            {
                return false;
            }
        }

        private Icon GetDefaultIcon()
        {
            try
            {
                // Try to get from entry assembly
                var assembly = Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    var iconPath = Path.Combine(
                        Path.GetDirectoryName(assembly.Location),
                        "app.ico");
                    if (File.Exists(iconPath))
                        return new Icon(iconPath);
                }

                // Fallback to system icon
                return SystemIcons.Application;
            }
            catch
            {
                return SystemIcons.Application;
            }
        }

        #endregion

        #region Show Notification Methods

        /// <summary>
        /// แสดง Notification จาก NotificationDto
        /// </summary>
        public void ShowNotification(NotificationDto notification)
        {
            if (notification == null || !_isInitialized) return;

            _currentNotification = notification;

            var icon = NotificationDto.GetIconForType(notification.TypeCode);
            var typeName = notification.TypeNameTH ?? NotificationDto.GetTypeNameTH(notification.TypeCode);
            var title = icon + " " + typeName;
            var message = notification.Title;
            
            if (!string.IsNullOrEmpty(notification.Message))
            {
                message += "\n" + notification.Message;
            }

            // เลือกวิธีแสดง
            if (UseToastNotification && _isWindows10OrLater)
            {
                // ลองใช้ Toast ก่อน
                if (!ShowToastNotification(title, message, notification.NotificationId.ToString()))
                {
                    // ถ้าไม่ได้ ใช้ Balloon แทน
                    ShowBalloonTip(title, message, GetToolTipIcon(notification.PriorityLevel));
                }
            }
            else
            {
                // ใช้ Balloon Tip
                ShowBalloonTip(title, message, GetToolTipIcon(notification.PriorityLevel));
            }
        }

        /// <summary>
        /// แสดง Balloon Tip (ใช้ได้ทุก Windows version)
        /// </summary>
        public void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info, int timeout = 5000)
        {
            if (!_isInitialized || _notifyIcon == null) return;

            try
            {
                // ตัดข้อความถ้ายาวเกินไป
                if (title != null && title.Length > 63)
                    title = title.Substring(0, 60) + "...";
                if (message != null && message.Length > 255)
                    message = message.Substring(0, 252) + "...";

                _notifyIcon.BalloonTipTitle = title ?? _appName;
                _notifyIcon.BalloonTipText = message ?? "";
                _notifyIcon.BalloonTipIcon = icon;
                _notifyIcon.ShowBalloonTip(timeout);

                Log("Balloon shown: " + title);
            }
            catch (Exception ex)
            {
                Log("ShowBalloonTip error: " + ex.Message);
            }
        }

        /// <summary>
        /// แสดง Windows Toast Notification ผ่าน PowerShell (Windows 10/11)
        /// </summary>
        private bool ShowToastNotification(string title, string message, string tag)
        {
            try
            {
                // สร้าง PowerShell script สำหรับ Toast
                var script = BuildToastPowerShellScript(title, message, tag);
                
                // รัน PowerShell
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-ExecutionPolicy Bypass -NoProfile -Command \"" + script + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit(3000); // Timeout 3 seconds
                    
                    if (process.ExitCode == 0)
                    {
                        Log("Toast shown via PowerShell: " + title);
                        return true;
                    }
                    else
                    {
                        var error = process.StandardError.ReadToEnd();
                        Log("Toast PowerShell error: " + error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("ShowToastNotification error: " + ex.Message);
                return false;
            }
        }

        private string BuildToastPowerShellScript(string title, string message, string tag)
        {
            // Escape quotes
            title = (title ?? "").Replace("'", "''").Replace("\"", "`\"");
            message = (message ?? "").Replace("'", "''").Replace("\"", "`\"").Replace("\n", " ");

            var script = @"
[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null

$template = @'
<toast>
    <visual>
        <binding template=""ToastGeneric"">
            <text>" + title + @"</text>
            <text>" + message + @"</text>
        </binding>
    </visual>
    <audio src=""ms-winsoundevent:Notification.Default""/>
</toast>
'@

$xml = New-Object Windows.Data.Xml.Dom.XmlDocument
$xml.LoadXml($template)

$toast = New-Object Windows.UI.Notifications.ToastNotification $xml
$notifier = [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('" + _appName + @"')
$notifier.Show($toast)
";
            // แปลงเป็น single line
            return script.Replace("\r\n", " ").Replace("\n", " ");
        }

        /// <summary>
        /// แสดง Info notification
        /// </summary>
        public void ShowInfo(string title, string message)
        {
            _currentNotification = null;
            ShowBalloonTip(title, message, ToolTipIcon.Info);
        }

        /// <summary>
        /// แสดง Warning notification
        /// </summary>
        public void ShowWarning(string title, string message)
        {
            _currentNotification = null;
            ShowBalloonTip(title, message, ToolTipIcon.Warning);
        }

        /// <summary>
        /// แสดง Error notification
        /// </summary>
        public void ShowError(string title, string message)
        {
            _currentNotification = null;
            ShowBalloonTip(title, message, ToolTipIcon.Error);
        }

        private ToolTipIcon GetToolTipIcon(NotificationPriority priority)
        {
            switch (priority)
            {
                case NotificationPriority.Urgent:
                case NotificationPriority.High:
                    return ToolTipIcon.Warning;
                case NotificationPriority.Low:
                    return ToolTipIcon.None;
                default:
                    return ToolTipIcon.Info;
            }
        }

        #endregion

        #region Tray Icon Control

        /// <summary>
        /// แสดง Tray Icon
        /// </summary>
        public void Show()
        {
            if (_notifyIcon != null)
                _notifyIcon.Visible = true;
        }

        /// <summary>
        /// ซ่อน Tray Icon
        /// </summary>
        public void Hide()
        {
            if (_notifyIcon != null)
                _notifyIcon.Visible = false;
        }

        /// <summary>
        /// อัปเดต Tooltip
        /// </summary>
        public void SetTooltip(string text)
        {
            if (_notifyIcon != null && !string.IsNullOrEmpty(text))
            {
                // Max length = 63
                _notifyIcon.Text = text.Length > 63 ? text.Substring(0, 60) + "..." : text;
            }
        }

        /// <summary>
        /// อัปเดต Unread count ใน Tooltip
        /// </summary>
        public void UpdateUnreadCount(int count)
        {
            var text = _appName + " Notification";
            if (count > 0)
                text += " (" + count + " รายการใหม่)";

            SetTooltip(text);
        }

        /// <summary>
        /// เปลี่ยน Icon
        /// </summary>
        public void SetIcon(Icon icon)
        {
            if (_notifyIcon != null && icon != null)
                _notifyIcon.Icon = icon;
        }

        #endregion

        #region Event Handlers

        private void OnTrayIconDoubleClick(object sender, EventArgs e)
        {
            var handler = TrayIconDoubleClicked;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnBalloonTipClicked(object sender, EventArgs e)
        {
            var handler = NotificationClicked;
            if (handler != null && _currentNotification != null)
            {
                handler(this, _currentNotification);
            }
            else if (handler != null)
            {
                handler(this, null);
            }
        }

        private void OnBalloonTipClosed(object sender, EventArgs e)
        {
            // Balloon closed
        }

        private void OnOpenMenuClick(object sender, EventArgs e)
        {
            var handler = OpenMenuClicked;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnDisableNotificationClick(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                // Toggle
                if (menuItem.Text == "ปิดการแจ้งเตือน")
                {
                    menuItem.Text = "เปิดการแจ้งเตือน";
                    UseToastNotification = false;
                    ShowBalloonTip("การแจ้งเตือน", "ปิดการแจ้งเตือนแล้ว", ToolTipIcon.Info, 2000);
                }
                else
                {
                    menuItem.Text = "ปิดการแจ้งเตือน";
                    UseToastNotification = _isWindows10OrLater;
                    ShowBalloonTip("การแจ้งเตือน", "เปิดการแจ้งเตือนแล้ว", ToolTipIcon.Info, 2000);
                }
            }
        }

        private void OnExitMenuClick(object sender, EventArgs e)
        {
            var handler = ExitMenuClicked;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion

        #region Helper Methods

        private void Log(string message)
        {
            Debug.WriteLine("[WindowsToast] " + message);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_notifyIcon != null)
                    {
                        _notifyIcon.Visible = false;
                        _notifyIcon.Dispose();
                        _notifyIcon = null;
                    }

                    if (_contextMenu != null)
                    {
                        _contextMenu.Dispose();
                        _contextMenu = null;
                    }
                }
                _disposed = true;
                _isInitialized = false;
            }
        }

        #endregion
    }
}
