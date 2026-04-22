// =============================================
// CMS Notification System
// Forms/NotificationAlertPopup.cs
// Popup Alert - Production Ready
// Auto-closes after 5 seconds
// =============================================

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Presentation.UserControls;

namespace CMS.Presentation.Forms
{
    /// <summary>
    /// Borderless popup form for notification alerts
    /// </summary>
    public class NotificationAlertPopup : Form
    {
        #region Fields

        private System.Windows.Forms.Timer _autoCloseTimer;
        private System.Windows.Forms.Timer _fadeTimer;
        private System.Windows.Forms.Timer _slideTimer;
        private NotificationItemControl _notificationControl;
        private NotificationDto _notification;
        private Panel _containerPanel;
        private double _opacity = 1.0;
        private bool _isPaused;
        private bool _isClosing;
        private int _targetX;
        
        private const int AUTO_CLOSE_SECONDS = 5;
        private const int FADE_INTERVAL_MS = 30;
        private const double FADE_STEP = 0.08;
        private const int POPUP_WIDTH = 400;
        private const int POPUP_HEIGHT = 105;
        private const int BORDER_RADIUS = 8;

        #endregion

        #region Events

        public event EventHandler<NotificationItemEventArgs> NotificationClicked;
        public event EventHandler<NotificationItemEventArgs> MarkAsReadClicked;
        public event EventHandler<NotificationItemEventArgs> DismissClicked;
        public event EventHandler PopupClosed;

        #endregion

        #region Properties

        public long NotificationId
        {
            get { return _notification != null ? _notification.NotificationId : 0; }
        }

        public NotificationDto Notification
        {
            get { return _notification; }
        }

        public NotificationItemControl NotificationControl
        {
            get { return _notificationControl; }
        }

        #endregion

        #region Constructor

        public NotificationAlertPopup()
        {
            InitializeForm();
            InitializeUserControl();
            InitializeTimers();
        }

        public NotificationAlertPopup(NotificationDto notification) : this()
        {
            SetNotification(notification);
        }

        #endregion

        #region Initialization

        private void InitializeForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.White;
            this.Size = new Size(POPUP_WIDTH, POPUP_HEIGHT);
            this.MinimumSize = new Size(POPUP_WIDTH, POPUP_HEIGHT);
            this.MaximumSize = new Size(POPUP_WIDTH, POPUP_HEIGHT);
            this.Padding = new Padding(0);
            this.DoubleBuffered = true;

            this.Region = CreateRoundedRegion(this.Width, this.Height, BORDER_RADIUS);
        }

        private void InitializeUserControl()
        {
            _containerPanel = new Panel();
            _containerPanel.Dock = DockStyle.Fill;
            _containerPanel.BackColor = Color.White;
            _containerPanel.Padding = new Padding(0);

            _notificationControl = new NotificationItemControl();
            _notificationControl.Dock = DockStyle.Fill;
            _notificationControl.BackColor = Color.White;
            _notificationControl.Size = new Size(POPUP_WIDTH, POPUP_HEIGHT);
            
            _notificationControl.NotificationClicked += OnNotificationClicked;
            _notificationControl.MarkAsReadClicked += OnMarkAsReadClicked;
            _notificationControl.DismissClicked += OnDismissClicked;

            _containerPanel.Controls.Add(_notificationControl);
            this.Controls.Add(_containerPanel);

            RegisterMouseEvents(this);
        }

        private void RegisterMouseEvents(Control parent)
        {
            parent.MouseEnter += OnMouseEnterForm;
            parent.MouseLeave += OnMouseLeaveForm;

            foreach (Control control in parent.Controls)
            {
                RegisterMouseEvents(control);
            }
        }

        private void InitializeTimers()
        {
            _autoCloseTimer = new System.Windows.Forms.Timer();
            _autoCloseTimer.Interval = AUTO_CLOSE_SECONDS * 1000;
            _autoCloseTimer.Tick += OnAutoCloseTimerTick;

            _fadeTimer = new System.Windows.Forms.Timer();
            _fadeTimer.Interval = FADE_INTERVAL_MS;
            _fadeTimer.Tick += OnFadeTimerTick;

            _slideTimer = new System.Windows.Forms.Timer();
            _slideTimer.Interval = 16;
            _slideTimer.Tick += OnSlideTimerTick;
        }

        private Region CreateRoundedRegion(int width, int height, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(width - radius * 2, height - radius * 2, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, height - radius * 2, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                return new Region(path);
            }
        }

        #endregion

        #region Public Methods

        public void SetNotification(NotificationDto notification)
        {
            _notification = notification;
            
            if (_notificationControl != null)
            {
                _notificationControl.SetNotification(notification);
            }
        }

        public void ShowPopup(int x, int y)
        {
            _targetX = x;
            
            this.Location = new Point(x + 60, y);
            this.Opacity = 1.0;
            _opacity = 1.0;
            _isClosing = false;
            _isPaused = false;

            this.Show();
            
            _slideTimer.Start();
            _autoCloseTimer.Start();
        }

        public void ClosePopup()
        {
            if (_isClosing) return;
            _isClosing = true;

            _autoCloseTimer.Stop();
            _slideTimer.Stop();
            _fadeTimer.Start();
        }

        public void CloseImmediately()
        {
            _autoCloseTimer.Stop();
            _fadeTimer.Stop();
            _slideTimer.Stop();
            _isClosing = true;

            var handler = PopupClosed;
            if (handler != null)
                handler(this, EventArgs.Empty);

            this.Close();
        }

        public void MoveTo(int x, int y, bool animate = true)
        {
            if (animate)
            {
                var moveTimer = new System.Windows.Forms.Timer();
                moveTimer.Interval = 16;
                var targetPoint = new Point(x, y);

                moveTimer.Tick += (s, e) =>
                {
                    if (this.IsDisposed)
                    {
                        moveTimer.Stop();
                        moveTimer.Dispose();
                        return;
                    }

                    var dx = (targetPoint.X - this.Location.X) / 3;
                    var dy = (targetPoint.Y - this.Location.Y) / 3;

                    if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1)
                    {
                        this.Location = targetPoint;
                        moveTimer.Stop();
                        moveTimer.Dispose();
                    }
                    else
                    {
                        this.Location = new Point(this.Location.X + dx, this.Location.Y + dy);
                    }
                };
                moveTimer.Start();
            }
            else
            {
                this.Location = new Point(x, y);
            }
        }

        public void MarkAsRead()
        {
            if (_notificationControl != null)
            {
                _notificationControl.MarkAsRead();
            }
            if (_notification != null)
            {
                _notification.IsRead = true;
            }
        }

        #endregion

        #region Timer Event Handlers

        private void OnAutoCloseTimerTick(object sender, EventArgs e)
        {
            if (!_isPaused)
            {
                _autoCloseTimer.Stop();
                ClosePopup();
            }
        }

        private void OnFadeTimerTick(object sender, EventArgs e)
        {
            _opacity -= FADE_STEP;
            
            if (_opacity <= 0)
            {
                _fadeTimer.Stop();
                this.Opacity = 0;

                var handler = PopupClosed;
                if (handler != null)
                    handler(this, EventArgs.Empty);

                this.Close();
            }
            else
            {
                this.Opacity = _opacity;
            }
        }

        private void OnSlideTimerTick(object sender, EventArgs e)
        {
            var dx = (_targetX - this.Location.X) / 3;
            
            if (Math.Abs(dx) <= 1)
            {
                this.Location = new Point(_targetX, this.Location.Y);
                _slideTimer.Stop();
            }
            else
            {
                this.Location = new Point(this.Location.X + dx, this.Location.Y);
            }
        }

        #endregion

        #region Mouse Event Handlers

        private void OnMouseEnterForm(object sender, EventArgs e)
        {
            _isPaused = true;

            if (_isClosing && _opacity < 1.0)
            {
                _fadeTimer.Stop();
                _isClosing = false;
                this.Opacity = 1.0;
                _opacity = 1.0;
            }
        }

        private void OnMouseLeaveForm(object sender, EventArgs e)
        {
            var clientPoint = this.PointToClient(Cursor.Position);
            
            if (!this.ClientRectangle.Contains(clientPoint))
            {
                _isPaused = false;

                if (!_isClosing)
                {
                    _autoCloseTimer.Stop();
                    _autoCloseTimer.Start();
                }
            }
        }

        #endregion

        #region UserControl Event Handlers

        private void OnNotificationClicked(object sender, NotificationItemEventArgs e)
        {
            var handler = NotificationClicked;
            if (handler != null)
                handler(this, e);

            CloseImmediately();
        }

        private void OnMarkAsReadClicked(object sender, NotificationItemEventArgs e)
        {
            _notificationControl.MarkAsRead();
            
            if (_notification != null)
                _notification.IsRead = true;

            var handler = MarkAsReadClicked;
            if (handler != null)
                handler(this, e);
        }

        private void OnDismissClicked(object sender, NotificationItemEventArgs e)
        {
            var handler = DismissClicked;
            if (handler != null)
                handler(this, e);

            CloseImmediately();
        }

        #endregion

        #region Override Methods

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return cp;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            if (_notificationControl != null)
            {
                _notificationControl.Refresh();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_autoCloseTimer != null)
                {
                    _autoCloseTimer.Stop();
                    _autoCloseTimer.Dispose();
                    _autoCloseTimer = null;
                }
                if (_fadeTimer != null)
                {
                    _fadeTimer.Stop();
                    _fadeTimer.Dispose();
                    _fadeTimer = null;
                }
                if (_slideTimer != null)
                {
                    _slideTimer.Stop();
                    _slideTimer.Dispose();
                    _slideTimer = null;
                }
                
                if (_notificationControl != null)
                {
                    _notificationControl.Dispose();
                    _notificationControl = null;
                }
                
                if (_containerPanel != null)
                {
                    _containerPanel.Dispose();
                    _containerPanel = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
