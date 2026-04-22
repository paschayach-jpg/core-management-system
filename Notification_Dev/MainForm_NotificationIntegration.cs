// =============================================
// E-Filing Application
// MainForm.cs - Notification Integration
// Production Ready for .NET Framework 4.5
// =============================================
//
// วิธีใช้งาน:
// 1. เพิ่ม NuGet Package: Newtonsoft.Json
// 2. Copy โฟลเดอร์ Notification, Forms, UserControls ไปไว้ใน Project
// 3. เพิ่ม code ด้านล่างนี้ใน MainForm.cs ของคุณ
//
// =============================================

using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Forms;
using E_Filing.Notification.Models;
using E_Filing.Notification.Services;

namespace E_Filing
{
    public partial class MainForm : Form
    {
        #region Notification Configuration

        // ========== แก้ไขค่าเหล่านี้ตามการตั้งค่าของคุณ ==========
        
        /// <summary>
        /// URL ของ Notification Server
        /// </summary>
        private readonly string NOTIFICATION_SERVER_URL = 
            ConfigurationManager.AppSettings["NotificationServerUrl"] ?? "https://localhost:7157";

        /// <summary>
        /// Application Code ที่ลงทะเบียนไว้ใน Server
        /// </summary>
        private readonly string NOTIFICATION_APP_CODE = 
            ConfigurationManager.AppSettings["NotificationAppCode"] ?? "E-FILING";

        /// <summary>
        /// API Key สำหรับ Authentication
        /// </summary>
        private readonly string NOTIFICATION_API_KEY = 
            ConfigurationManager.AppSettings["NotificationApiKey"] ?? "ef_4FE36BA8-D3C7-4C00-A41A-E1648E567B30";

        // ===========================================================

        // Notification badge (optional)
        private Label lblNotificationBadge;
        private Button btnNotification;
        private bool _notificationInitialized = false;

        #endregion

        #region Notification Initialization

        /// <summary>
        /// เรียก method นี้หลังจาก Login สำเร็จ
        /// ใส่ใน GetMenuControl หลังจากได้ข้อมูล user แล้ว
        /// </summary>
        /// <param name="userAd">AD Username (เช่น sasithorn, somchai_k)</param>
        /// <param name="displayName">ชื่อแสดง (optional)</param>
        /// <param name="department">แผนก (optional)</param>
        private async Task InitializeNotificationSystemAsync(string userAd, string displayName = null, string department = null)
        {
            if (_notificationInitialized)
            {
                System.Diagnostics.Debug.WriteLine("[MainForm] Notification already initialized");
                return;
            }

            if (string.IsNullOrEmpty(userAd))
            {
                System.Diagnostics.Debug.WriteLine("[MainForm] UserAd is empty, skipping notification init");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("[MainForm] Initializing notification system...");
                System.Diagnostics.Debug.WriteLine("[MainForm] Server: " + NOTIFICATION_SERVER_URL);
                System.Diagnostics.Debug.WriteLine("[MainForm] UserAd: " + userAd);

                // Initialize NotificationManager
                NotificationManager.Instance.Initialize(
                    this,
                    NOTIFICATION_SERVER_URL,
                    NOTIFICATION_APP_CODE,
                    NOTIFICATION_API_KEY
                );

                // Subscribe to events
                NotificationManager.Instance.NotificationReceived += OnNotificationReceived;
                NotificationManager.Instance.UnreadCountChanged += OnUnreadCountChanged;
                NotificationManager.Instance.NotificationClicked += OnNotificationClicked;
                NotificationManager.Instance.ConnectionStateChanged += OnConnectionStateChanged;
                NotificationManager.Instance.Connected += OnNotificationConnected;
                NotificationManager.Instance.Error += OnNotificationError;

                // Connect to server
                await NotificationManager.Instance.ConnectAsync(userAd);

                _notificationInitialized = true;
                System.Diagnostics.Debug.WriteLine("[MainForm] Notification system initialized successfully!");

                // Update UI
                UpdateNotificationConnectionStatus(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[MainForm] Notification init error: " + ex.Message);
                UpdateNotificationConnectionStatus(false);

                // ไม่ throw exception เพราะไม่อยากให้ app crash ถ้า notification server ไม่ทำงาน
                // แค่ log error และแสดงสถานะใน UI
            }
        }

        /// <summary>
        /// สร้าง Notification Badge UI (optional)
        /// เรียกใน InitializeComponent หรือ MainForm_Load
        /// </summary>
        private void InitializeNotificationBadgeUI()
        {
            // Notification Button
            btnNotification = new Button();
            btnNotification.Text = "🔔";
            btnNotification.Font = new System.Drawing.Font("Segoe UI", 12F);
            btnNotification.Size = new System.Drawing.Size(40, 35);
            btnNotification.FlatStyle = FlatStyle.Flat;
            btnNotification.FlatAppearance.BorderSize = 0;
            btnNotification.Cursor = Cursors.Hand;
            btnNotification.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNotification.Location = new System.Drawing.Point(this.ClientSize.Width - 55, 5);
            btnNotification.Click += BtnNotification_Click;
            btnNotification.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(btnNotification);
            btnNotification.BringToFront();

            // Badge Label
            lblNotificationBadge = new Label();
            lblNotificationBadge.Text = "0";
            lblNotificationBadge.Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Bold);
            lblNotificationBadge.ForeColor = System.Drawing.Color.White;
            lblNotificationBadge.BackColor = System.Drawing.Color.FromArgb(244, 67, 54);
            lblNotificationBadge.Size = new System.Drawing.Size(18, 18);
            lblNotificationBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblNotificationBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblNotificationBadge.Location = new System.Drawing.Point(btnNotification.Right - 12, btnNotification.Top - 3);
            lblNotificationBadge.Visible = false;
            this.Controls.Add(lblNotificationBadge);
            lblNotificationBadge.BringToFront();
        }

        #endregion

        #region Notification Event Handlers

        private void OnNotificationReceived(object sender, NotificationDto notification)
        {
            // Popup แสดงอัตโนมัติโดย NotificationManager
            // ถ้าต้องการทำอะไรเพิ่มเติม ทำได้ที่นี่
            System.Diagnostics.Debug.WriteLine("[MainForm] Notification received: " + notification.Title);
        }

        private void OnUnreadCountChanged(object sender, int count)
        {
            // Update badge
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateBadge(count)));
            }
            else
            {
                UpdateBadge(count);
            }
        }

        private void OnNotificationClicked(object sender, NotificationDto notification)
        {
            System.Diagnostics.Debug.WriteLine("[MainForm] Notification clicked: " + notification.NotificationId);

            // Navigate ไปหน้าที่เกี่ยวข้องตาม ReferenceType
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => HandleNotificationNavigation(notification)));
            }
            else
            {
                HandleNotificationNavigation(notification);
            }
        }

        private void OnConnectionStateChanged(object sender, NotificationConnectionState state)
        {
            System.Diagnostics.Debug.WriteLine("[MainForm] Connection state: " + state);

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => 
                    UpdateNotificationConnectionStatus(state == NotificationConnectionState.Connected)));
            }
            else
            {
                UpdateNotificationConnectionStatus(state == NotificationConnectionState.Connected);
            }
        }

        private void OnNotificationConnected(object sender, string connectionId)
        {
            System.Diagnostics.Debug.WriteLine("[MainForm] Connected with ID: " + connectionId);
        }

        private void OnNotificationError(object sender, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("[MainForm] Notification error: " + ex.Message);
        }

        private void BtnNotification_Click(object sender, EventArgs e)
        {
            // เปิดหน้า Notification List
            OpenNotificationListForm();
        }

        #endregion

        #region Notification Helper Methods

        private void UpdateBadge(int count)
        {
            if (lblNotificationBadge == null) return;

            if (count > 0)
            {
                lblNotificationBadge.Text = count > 99 ? "99+" : count.ToString();
                lblNotificationBadge.Visible = true;
            }
            else
            {
                lblNotificationBadge.Visible = false;
            }
        }

        private void UpdateNotificationConnectionStatus(bool isConnected)
        {
            if (btnNotification != null)
            {
                btnNotification.ForeColor = isConnected
                    ? System.Drawing.Color.FromArgb(33, 150, 243)
                    : System.Drawing.Color.Gray;
            }

            // Update status bar if exists
            // toolStripStatusLabel1.Text = isConnected ? "🔔 เชื่อมต่อแล้ว" : "⚠️ ไม่ได้เชื่อมต่อ";
        }

        private void HandleNotificationNavigation(NotificationDto notification)
        {
            // Navigate ตาม ReferenceType
            switch (notification.ReferenceType?.ToLower())
            {
                case "claim":
                    // เปิดหน้า Claim Detail
                    // OpenClaimDetailForm(notification.ReferenceId);
                    MessageBox.Show("เปิดเคลม: " + notification.ReferenceId, "Navigation", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                case "policy":
                    // เปิดหน้า Policy Detail
                    // OpenPolicyDetailForm(notification.ReferenceId);
                    break;

                case "document":
                    // เปิดหน้า Document Detail
                    // OpenDocumentDetailForm(notification.ReferenceId);
                    break;

                case "transfer":
                    // เปิดหน้า Transfer
                    // OpenTransferForm(notification.ReferenceId);
                    break;

                default:
                    // เปิดหน้า Notification List
                    OpenNotificationListForm();
                    break;
            }

            // Mark as read
            Task.Run(async () =>
            {
                await NotificationManager.Instance.MarkAsReadAsync(notification.NotificationId);
            });
        }

        private void OpenNotificationListForm()
        {
            // เปิด Form แสดงรายการ Notification ทั้งหมด
            // var form = new NotificationListForm();
            // form.ShowDialog(this);

            MessageBox.Show("เปิดหน้ารายการแจ้งเตือน", "Notification", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Send Notification Examples

        /// <summary>
        /// ตัวอย่างการส่ง Notification เมื่อมีการ Assign งาน
        /// </summary>
        public async Task SendAssignNotificationAsync(string recipientUserAd, string claimNo, string claimId)
        {
            if (!NotificationManager.Instance.IsConnected) return;

            await NotificationManager.Instance.SendNotificationAsync(
                typeCode: "ASSIGN_NOTE",
                title: "คุณได้รับมอบหมายงาน: " + claimNo,
                message: _displayName + " ได้มอบหมายงานเคลมให้คุณดำเนินการ",
                recipientUserAd: recipientUserAd,
                senderUserAd: _samAccountName,
                referenceType: "claim",
                referenceId: claimId,
                referenceCode: claimNo,
                priority: 2
            );
        }

        /// <summary>
        /// ตัวอย่างการส่ง Notification เมื่อมี Comment ใหม่
        /// </summary>
        public async Task SendCommentNotificationAsync(string recipientUserAd, string claimNo, string claimId, string comment)
        {
            if (!NotificationManager.Instance.IsConnected) return;

            var commentPreview = comment.Length > 50 ? comment.Substring(0, 50) + "..." : comment;

            await NotificationManager.Instance.SendNotificationAsync(
                typeCode: "COMMENT",
                title: "ความคิดเห็นใหม่: " + claimNo,
                message: _displayName + ": " + commentPreview,
                recipientUserAd: recipientUserAd,
                senderUserAd: _samAccountName,
                referenceType: "claim",
                referenceId: claimId,
                referenceCode: claimNo,
                priority: 2
            );
        }

        /// <summary>
        /// ตัวอย่างการส่ง Notification ขออนุมัติ
        /// </summary>
        public async Task SendApprovalRequestAsync(string approverUserAd, string documentNo, string documentId)
        {
            if (!NotificationManager.Instance.IsConnected) return;

            await NotificationManager.Instance.SendNotificationAsync(
                typeCode: "APPROVAL_DOCUMENT_SUBMIT",
                title: "ขออนุมัติ: " + documentNo,
                message: _displayName + " ส่งเอกสารขออนุมัติ",
                recipientUserAd: approverUserAd,
                senderUserAd: _samAccountName,
                referenceType: "document",
                referenceId: documentId,
                referenceCode: documentNo,
                priority: 3 // High
            );
        }

        #endregion

        #region Integration with GetMenuControl

        /*
        // ========================================
        // เพิ่ม code นี้ใน GetMenuControl method
        // หลังจากได้ข้อมูล user แล้ว
        // ========================================

        private async Task GetMenuControl(string accessToken, int programId)
        {
            // ... existing code ...

            if (loginResult != null && loginResult.EmployeeInfo != null)
            {
                // ... existing code to set user info ...

                _displayName = fullname;
                _samAccountName = loginResult.UserName;
                _department = loginResult.EmployeeInfo.Department_Name_En;

                // ===== เพิ่มบรรทัดนี้ =====
                await InitializeNotificationSystemAsync(_samAccountName, _displayName, _department);
                // ==========================
            }
        }
        */

        #endregion

        #region App.config Settings

        /*
        // ========================================
        // เพิ่มใน App.config
        // ========================================

        <appSettings>
            <!-- Notification Settings -->
            <add key="NotificationServerUrl" value="https://localhost:7157" />
            <add key="NotificationAppCode" value="E-FILING" />
            <add key="NotificationApiKey" value="ef_4FE36BA8-D3C7-4C00-A41A-E1648E567B30" />
        </appSettings>
        */

        #endregion
    }
}
