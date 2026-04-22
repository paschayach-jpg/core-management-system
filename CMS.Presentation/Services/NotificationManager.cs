// =============================================
// CMS Notification System
// Services/NotificationManager.cs
// Singleton Manager - Production Ready
// Compatible with .NET Framework 4.5
// =============================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Presentation.Forms;
using CMS.Services;

namespace CMS.Presentation.Services
{
    /// <summary>
    /// Singleton manager for notification system
    /// </summary>
    public sealed class NotificationManager : IDisposable
    {
        #region Singleton

        private static volatile NotificationManager _instance;
        private static readonly object _syncRoot = new object();

        public static NotificationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new NotificationManager();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Fields

        private NotificationClient _client;
        private Form _mainForm;
        private List<NotificationAlertPopup> _activePopups;
        private Queue<NotificationDto> _pendingQueue;
        private bool _isInitialized;
        private bool _disposed;
        private System.Windows.Forms.Timer _cleanupTimer;

        private const int MAX_VISIBLE_POPUPS = 3;
        private const int POPUP_SPACING = 10;
        private const int POPUP_HEIGHT = 110;
        private const int POPUP_OFFSET_TOP = 60;
        private const int POPUP_OFFSET_RIGHT = 15;

        #endregion

        #region Properties

        public bool IsConnected 
        { 
            get { return _client != null && _client.IsConnected; } 
        }

        public NotificationConnectionState ConnectionState
        {
            get { return _client != null ? _client.ConnectionState : NotificationConnectionState.Disconnected; }
        }

        public string CurrentUserAd
        {
            get { return _client != null ? _client.CurrentUserAd : null; }
        }

        public int UnreadCount { get; private set; }

        public bool PlaySound { get; set; }
        public bool ShowPopups { get; set; }
        public bool ShowToastNotification { get; set; }

        public int PollIntervalMs
        {
            get { return _client != null ? _client.PollIntervalMs : 3000; }
            set { if (_client != null) _client.PollIntervalMs = value; }
        }

        #endregion

        #region Events

        public event EventHandler<NotificationDto> NotificationReceived;
        public event EventHandler<int> UnreadCountChanged;
        public event EventHandler<NotificationDto> NotificationClicked;
        public event EventHandler<NotificationConnectionState> ConnectionStateChanged;
        public event EventHandler<Exception> Error;
        public event EventHandler<string> Connected;

        #endregion

        #region Constructor

        private NotificationManager()
        {
            _activePopups = new List<NotificationAlertPopup>();
            _pendingQueue = new Queue<NotificationDto>();
            
            PlaySound = true;
            ShowPopups = true;
            ShowToastNotification = true;

            _cleanupTimer = new System.Windows.Forms.Timer();
            _cleanupTimer.Interval = 5000;
            _cleanupTimer.Tick += CleanupTimer_Tick;
        }

        #endregion

        #region Initialization

        public void Initialize(Form mainForm, string serverUrl, string appCode, string apiKey)
        {
            if (_isInitialized)
            {
                Log("Already initialized, reinitializing...");
                Dispose();
                _disposed = false;
            }

            if (mainForm == null)
                throw new ArgumentNullException("mainForm");

            _mainForm = mainForm;

            _client = new NotificationClient(serverUrl, appCode, apiKey);

            _client.NotificationReceived += Client_NotificationReceived;
            _client.UnreadCountUpdated += Client_UnreadCountUpdated;
            _client.PendingNotificationsReceived += Client_PendingNotificationsReceived;
            _client.ConnectionStateChanged += Client_ConnectionStateChanged;
            _client.ErrorOccurred += Client_ErrorOccurred;
            _client.Connected += Client_Connected;

            _mainForm.FormClosing += MainForm_FormClosing;
            _mainForm.Resize += MainForm_Resize;
            _mainForm.Move += MainForm_Move;
            _mainForm.LocationChanged += MainForm_LocationChanged;

            _cleanupTimer.Start();
            _isInitialized = true;

            Log("Initialized successfully");
        }

        public async Task ConnectAsync(string userAd)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("NotificationManager not initialized. Call Initialize() first.");

            if (string.IsNullOrEmpty(userAd))
                throw new ArgumentNullException("userAd");

            Log("Connecting as: " + userAd);
            await _client.ConnectAsync(userAd, Environment.MachineName);
        }

        public void Disconnect()
        {
            if (_client != null)
            {
                _client.Disconnect();
            }
            CloseAllPopups();
            Log("Disconnected");
        }

        #endregion

        #region Popup Management

        public void ShowNotificationPopup(NotificationDto notification)
        {
            if (!ShowPopups || _mainForm == null || _mainForm.IsDisposed)
                return;

            if (_mainForm.InvokeRequired)
            {
                _mainForm.BeginInvoke(new Action(() => ShowNotificationPopup(notification)));
                return;
            }

            try
            {
                if (_activePopups.Any(p => !p.IsDisposed && p.NotificationId == notification.NotificationId))
                    return;

                if (_activePopups.Count(p => !p.IsDisposed) >= MAX_VISIBLE_POPUPS)
                {
                    _pendingQueue.Enqueue(notification);
                    Log("Popup queued: " + notification.NotificationId);
                    return;
                }

                var popup = new NotificationAlertPopup(notification);
                popup.NotificationClicked += Popup_NotificationClicked;
                popup.MarkAsReadClicked += Popup_MarkAsReadClicked;
                popup.DismissClicked += Popup_DismissClicked;
                popup.PopupClosed += Popup_Closed;

                _activePopups.Add(popup);

                var position = CalculatePopupPosition(_activePopups.Count(p => !p.IsDisposed) - 1);
                popup.ShowPopup(position.X, position.Y);

                Log("Popup shown: " + notification.NotificationId);

                if (PlaySound)
                {
                    PlayNotificationSound(notification.PriorityLevel);
                }
            }
            catch (Exception ex)
            {
                Log("ShowPopup error: " + ex.Message);
            }
        }

        public void CloseAllPopups()
        {
            if (_mainForm != null && _mainForm.InvokeRequired)
            {
                _mainForm.BeginInvoke(new Action(CloseAllPopups));
                return;
            }

            try
            {
                var popups = _activePopups.ToList();
                foreach (var popup in popups)
                {
                    if (popup != null && !popup.IsDisposed)
                    {
                        popup.CloseImmediately();
                    }
                }
                _activePopups.Clear();
                _pendingQueue.Clear();
            }
            catch (Exception ex)
            {
                Log("CloseAllPopups error: " + ex.Message);
            }
        }

        private Point CalculatePopupPosition(int index)
        {
            if (_mainForm == null || _mainForm.IsDisposed)
                return new Point(100, 100);

            int x = _mainForm.Right - 410 - POPUP_OFFSET_RIGHT;
            int y = _mainForm.Top + POPUP_OFFSET_TOP + (index * (POPUP_HEIGHT + POPUP_SPACING));

            var screen = Screen.FromControl(_mainForm);
            if (x < screen.WorkingArea.Left)
                x = screen.WorkingArea.Left + 10;
            if (y < screen.WorkingArea.Top)
                y = screen.WorkingArea.Top + 10;
            if (x + 400 > screen.WorkingArea.Right)
                x = screen.WorkingArea.Right - 410;

            return new Point(x, y);
        }

        private void RearrangePopups()
        {
            if (_mainForm == null || _mainForm.IsDisposed)
                return;

            if (_mainForm.InvokeRequired)
            {
                _mainForm.BeginInvoke(new Action(RearrangePopups));
                return;
            }

            try
            {
                _activePopups.RemoveAll(p => p == null || p.IsDisposed);

                for (int i = 0; i < _activePopups.Count; i++)
                {
                    var popup = _activePopups[i];
                    if (popup != null && !popup.IsDisposed)
                    {
                        var position = CalculatePopupPosition(i);
                        popup.MoveTo(position.X, position.Y);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("RearrangePopups error: " + ex.Message);
            }
        }

        private void ShowNextPendingPopup()
        {
            if (_pendingQueue.Count > 0 && _activePopups.Count(p => !p.IsDisposed) < MAX_VISIBLE_POPUPS)
            {
                var notification = _pendingQueue.Dequeue();
                ShowNotificationPopup(notification);
            }
        }

        private void PlayNotificationSound(NotificationPriority priority)
        {
            try
            {
                if (priority == NotificationPriority.Urgent || priority == NotificationPriority.High)
                {
                    SystemSounds.Exclamation.Play();
                }
                else
                {
                    SystemSounds.Asterisk.Play();
                }
            }
            catch { }
        }

        #endregion

        #region Public Methods

        public async Task<bool> MarkAsReadAsync(long notificationId)
        {
            if (_client == null || !_client.IsConnected)
                return false;

            return await _client.MarkAsReadAsync(notificationId);
        }

        public async Task<int> MarkAllAsReadAsync(string typeCode = null)
        {
            if (_client == null || !_client.IsConnected)
                return 0;

            return await _client.MarkAllAsReadAsync(typeCode);
        }

        public async Task<PagedResultNotification<NotificationDto>> GetNotificationsAsync(
            string typeCode = null,
            bool? isRead = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            if (_client == null)
                return new PagedResultNotification<NotificationDto>();

            return await _client.GetNotificationsAsync(typeCode, isRead, pageNumber, pageSize);
        }

        public async Task<List<UnreadCountDto>> GetUnreadCountAsync()
        {
            if (_client == null || !_client.IsConnected)
                return new List<UnreadCountDto>();

            return await _client.GetUnreadCountAsync();
        }

        public async Task<NotificationDto> SendNotificationAsync(
            string typeCode,
            string title,
            string message,
            string recipientUserAd,
            string senderUserAd = null,
            string referenceType = null,
            string referenceId = null,
            string referenceCode = null,
            int priority = 2)
        {
            if (_client == null)
                throw new InvalidOperationException("NotificationManager not initialized");

            return await _client.SendNotificationAsync(
                typeCode, title, message, recipientUserAd,
                senderUserAd, referenceType, referenceId, referenceCode, priority);
        }

        public async Task RefreshUnreadCountAsync()
        {
            if (_client != null && _client.IsConnected)
            {
                await _client.RefreshUnreadCountAsync();
            }
        }

        #endregion

        #region Event Handlers - Client

        private void Client_NotificationReceived(object sender, NotificationEventArgs e)
        {
            Log("Notification received: " + e.Notification.NotificationId);

            bool isMinimized = _mainForm != null && _mainForm.WindowState == FormWindowState.Minimized;

            if (ShowPopups && !isMinimized)
            {
                ShowNotificationPopup(e.Notification);
            }

            if (PlaySound)
            {
                PlayNotificationSound(e.Notification.PriorityLevel);
            }

            var handler = NotificationReceived;
            if (handler != null)
            {
                InvokeOnMainThread(() => handler(this, e.Notification));
            }
        }

        private void Client_UnreadCountUpdated(object sender, UnreadCountEventArgs e)
        {
            UnreadCount = e.TotalCount;
            Log("Unread count: " + UnreadCount);

            var handler = UnreadCountChanged;
            if (handler != null)
            {
                InvokeOnMainThread(() => handler(this, UnreadCount));
            }
        }

        private void Client_PendingNotificationsReceived(object sender, PendingNotificationsEventArgs e)
        {
            Log("Pending notifications: " + e.Notifications.Count);

            foreach (var notification in e.Notifications.Take(MAX_VISIBLE_POPUPS))
            {
                ShowNotificationPopup(notification);
            }
        }

        private void Client_ConnectionStateChanged(object sender, ConnectionStateEventArgs e)
        {
            Log("Connection state: " + e.NewState);

            var handler = ConnectionStateChanged;
            if (handler != null)
            {
                InvokeOnMainThread(() => handler(this, e.NewState));
            }
        }

        private void Client_ErrorOccurred(object sender, NotificationErrorEventArgs e)
        {
            Log("Error: " + e.Context + " - " + e.Exception.Message);

            var handler = Error;
            if (handler != null)
            {
                InvokeOnMainThread(() => handler(this, e.Exception));
            }
        }

        private void Client_Connected(object sender, ConnectedEventArgs e)
        {
            Log("Connected: " + e.ConnectionId);

            var handler = Connected;
            if (handler != null)
            {
                InvokeOnMainThread(() => handler(this, e.ConnectionId));
            }
        }

        #endregion

        #region Event Handlers - Popup

        private void Popup_NotificationClicked(object sender, NotificationItemEventArgs e)
        {
            var popup = sender as NotificationAlertPopup;
            if (popup != null)
            {
                var handler = NotificationClicked;
                if (handler != null)
                {
                    var notification = new NotificationDto
                    {
                        NotificationId = e.NotificationId,
                        ActionUrl = e.ActionUrl,
                        ReferenceType = e.ReferenceType,
                        ReferenceId = e.ReferenceId
                    };
                    handler(this, notification);
                }
            }
        }

        private async void Popup_MarkAsReadClicked(object sender, NotificationItemEventArgs e)
        {
            await MarkAsReadAsync(e.NotificationId);
        }

        private void Popup_DismissClicked(object sender, NotificationItemEventArgs e)
        {
            var popup = sender as NotificationAlertPopup;
            if (popup != null)
            {
                _activePopups.Remove(popup);
                RearrangePopups();
                ShowNextPendingPopup();
            }
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            var popup = sender as NotificationAlertPopup;
            if (popup != null)
            {
                _activePopups.Remove(popup);
                RearrangePopups();
                ShowNextPendingPopup();
            }
        }

        #endregion

        #region Event Handlers - Form

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (_mainForm.WindowState == FormWindowState.Minimized)
            {
                foreach (var popup in _activePopups.Where(p => !p.IsDisposed))
                {
                    popup.Hide();
                }
            }
            else
            {
                foreach (var popup in _activePopups.Where(p => !p.IsDisposed))
                {
                    popup.Show();
                }
                RearrangePopups();
            }
        }

        private void MainForm_Move(object sender, EventArgs e)
        {
            RearrangePopups();
        }

        private void MainForm_LocationChanged(object sender, EventArgs e)
        {
            RearrangePopups();
        }

        private void CleanupTimer_Tick(object sender, EventArgs e)
        {
            _activePopups.RemoveAll(p => p == null || p.IsDisposed);
        }

        #endregion

        #region Helpers

        private void InvokeOnMainThread(Action action)
        {
            if (_mainForm != null && !_mainForm.IsDisposed)
            {
                if (_mainForm.InvokeRequired)
                {
                    _mainForm.BeginInvoke(action);
                }
                else
                {
                    action();
                }
            }
        }

        private void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine("[NotificationManager] " + message);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                _cleanupTimer.Stop();
                _cleanupTimer.Dispose();

                CloseAllPopups();
                Disconnect();

                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }

                _isInitialized = false;
                _disposed = true;
            }
        }

        #endregion
    }
}
