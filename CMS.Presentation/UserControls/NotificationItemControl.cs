// =============================================
// CMS Notification System
// UserControls/NotificationItemControl.cs
// Production Ready for .NET Framework 4.5
// =============================================

using System;
using System.Drawing;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation.UserControls
{
    /// <summary>
    /// UserControl for displaying a single notification item
    /// </summary>
    public partial class NotificationItemControl : UserControl
    {
        #region Properties

        public long NotificationId { get; private set; }
        public string ActionUrl { get; private set; }
        public string ReferenceType { get; private set; }
        public string ReferenceId { get; private set; }
        public bool IsRead { get; private set; }

        private NotificationDto _notification;

        #endregion

        #region Events

        public event EventHandler<NotificationItemEventArgs> NotificationClicked;
        public event EventHandler<NotificationItemEventArgs> MarkAsReadClicked;
        public event EventHandler<NotificationItemEventArgs> DismissClicked;

        #endregion

        #region Constructor

        public NotificationItemControl()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        public NotificationItemControl(NotificationDto notification) : this()
        {
            SetNotification(notification);
        }

        #endregion

        #region Setup

        private void SetupEventHandlers()
        {
            this.pnlMain.Click += OnItemClick;
            this.pnlContent.Click += OnItemClick;
            this.lblTitle.Click += OnItemClick;
            this.lblMessage.Click += OnItemClick;
            this.lblIcon.Click += OnItemClick;

            this.pnlMain.MouseEnter += OnMouseEnterControl;
            this.pnlMain.MouseLeave += OnMouseLeaveControl;
            this.pnlContent.MouseEnter += OnMouseEnterControl;
            this.pnlContent.MouseLeave += OnMouseLeaveControl;

            RegisterChildMouseEvents(this);

            this.btnMarkRead.Click += BtnMarkRead_Click;
            this.btnDismiss.Click += BtnDismiss_Click;
        }

        private void RegisterChildMouseEvents(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                control.MouseEnter += OnMouseEnterControl;
                control.MouseLeave += OnMouseLeaveControl;
                if (control.HasChildren)
                {
                    RegisterChildMouseEvents(control);
                }
            }
        }

        #endregion

        #region Public Methods

        public void SetNotification(NotificationDto notification)
        {
            if (notification == null) return;

            _notification = notification;
            NotificationId = notification.NotificationId;
            ActionUrl = notification.ActionUrl;
            ReferenceType = notification.ReferenceType;
            ReferenceId = notification.ReferenceId;
            IsRead = notification.IsRead;

            lblIcon.Text = NotificationDto.GetIconForType(notification.TypeCode);

            lblType.Text = !string.IsNullOrEmpty(notification.TypeNameTH)
                ? notification.TypeNameTH
                : NotificationDto.GetTypeNameTH(notification.TypeCode);

            var colorCode = !string.IsNullOrEmpty(notification.ColorCode)
                ? notification.ColorCode
                : NotificationDto.GetColorForType(notification.TypeCode);

            try
            {
                var color = ColorTranslator.FromHtml(colorCode);
                lblType.ForeColor = color;
                pnlColorBar.BackColor = color;
            }
            catch
            {
                lblType.ForeColor = Color.FromArgb(33, 150, 243);
                pnlColorBar.BackColor = Color.FromArgb(33, 150, 243);
            }

            SetPriority(notification.PriorityLevel);

            lblTitle.Text = notification.Title ?? "";
            lblMessage.Text = notification.Message ?? "";
            
            // Display time with validation for default DateTime values
            if (notification.CreatedDate == DateTime.MinValue || notification.CreatedDate == default(DateTime))
            {
                lblTime.Text = "เมื่อสักครู่";
                toolTip.SetToolTip(lblTime, "ไม่มีข้อมูลเวลา");
            }
            else
            {
                lblTime.Text = notification.TimeAgo;
                toolTip.SetToolTip(lblTime, notification.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss"));
            }

            if (!string.IsNullOrEmpty(notification.ReferenceCode))
            {
                lblReference.Text = "อ้างอิง: " + notification.ReferenceCode;
                lblReference.Visible = true;
            }
            else
            {
                lblReference.Visible = false;
            }

            UpdateReadStatus();
        }

        public void MarkAsRead()
        {
            IsRead = true;
            if (_notification != null)
                _notification.IsRead = true;
            UpdateReadStatus();
        }

        #endregion

        #region Private Methods

        private void SetPriority(NotificationPriority priority)
        {
            switch (priority)
            {
                case NotificationPriority.Urgent:
                    lblPriority.Text = "ด่วนมาก";
                    lblPriority.BackColor = Color.FromArgb(244, 67, 54);
                    lblPriority.ForeColor = Color.White;
                    lblPriority.Visible = true;
                    break;
                case NotificationPriority.High:
                    lblPriority.Text = "ด่วน";
                    lblPriority.BackColor = Color.FromArgb(255, 152, 0);
                    lblPriority.ForeColor = Color.White;
                    lblPriority.Visible = true;
                    break;
                default:
                    lblPriority.Visible = false;
                    break;
            }
        }

        private void UpdateReadStatus()
        {
            if (IsRead)
            {
                pnlMain.BackColor = Color.FromArgb(250, 250, 250);
                lblTitle.Font = new Font(lblTitle.Font, FontStyle.Regular);
                lblTitle.ForeColor = Color.FromArgb(97, 97, 97);
                pnlUnreadIndicator.Visible = false;
                btnMarkRead.Visible = false;
            }
            else
            {
                pnlMain.BackColor = Color.White;
                lblTitle.Font = new Font(lblTitle.Font, FontStyle.Bold);
                lblTitle.ForeColor = Color.FromArgb(33, 33, 33);
                pnlUnreadIndicator.Visible = true;
                pnlUnreadIndicator.BackColor = pnlColorBar.BackColor;
                btnMarkRead.Visible = true;
            }
        }

        private NotificationItemEventArgs CreateEventArgs()
        {
            return new NotificationItemEventArgs
            {
                NotificationId = NotificationId,
                ActionUrl = ActionUrl,
                ReferenceType = ReferenceType,
                ReferenceId = ReferenceId
            };
        }

        #endregion

        #region Event Handlers

        private void OnItemClick(object sender, EventArgs e)
        {
            var handler = NotificationClicked;
            if (handler != null)
            {
                handler(this, CreateEventArgs());
            }
        }

        private void OnMouseEnterControl(object sender, EventArgs e)
        {
            pnlMain.BackColor = IsRead
                ? Color.FromArgb(245, 245, 245)
                : Color.FromArgb(232, 245, 253);
            pnlActions.Visible = true;
        }

        private void OnMouseLeaveControl(object sender, EventArgs e)
        {
            var clientPoint = pnlMain.PointToClient(Cursor.Position);
            if (!pnlMain.ClientRectangle.Contains(clientPoint))
            {
                pnlMain.BackColor = IsRead
                    ? Color.FromArgb(250, 250, 250)
                    : Color.White;
                pnlActions.Visible = false;
            }
        }

        private void BtnMarkRead_Click(object sender, EventArgs e)
        {
            // First, raise the MarkAsReadClicked event to update the server
            var markReadHandler = MarkAsReadClicked;
            if (markReadHandler != null)
            {
                markReadHandler(this, CreateEventArgs());
            }

            // Then, raise the DismissClicked event to close/remove the notification
            var dismissHandler = DismissClicked;
            if (dismissHandler != null)
            {
                dismissHandler(this, CreateEventArgs());
            }
        }

        private void BtnDismiss_Click(object sender, EventArgs e)
        {
            var handler = DismissClicked;
            if (handler != null)
            {
                handler(this, CreateEventArgs());
            }
        }

        #endregion
    }
}
