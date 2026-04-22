// =============================================
// CMS Notification System
// Services/NotificationClient.cs
// Long Polling Client - Production Ready
// Compatible with .NET Framework 4.5
// =============================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CMS.Domain.Models;

namespace CMS.Services
{
    /// <summary>
    /// Notification Client using Long Polling
    /// Production ready for .NET Framework 4.5
    /// </summary>
    public class NotificationClient : IDisposable
    {
        #region Fields

        private readonly string _serverUrl;
        private readonly string _appCode;
        private readonly string _apiKey;
        private string _userAd;
        private string _deviceInfo;
        private bool _disposed;
        private volatile bool _isPolling;
        private CancellationTokenSource _pollingCts;
        private Thread _pollingThread;
        private long _lastNotificationId;
        private DateTime _lastPollTime;
        private int _consecutiveErrors;
        private HttpClient _httpClient;
        private readonly object _lockObject = new object();

        // Configuration
        private int _pollIntervalMs = 3000;
        private int _maxConsecutiveErrors = 10;
        private int _maxBackoffMs = 60000;
        private int _connectionTimeoutSec = 30;

        #endregion

        #region Properties

        /// <summary>
        /// Current connection state
        /// </summary>
        public NotificationConnectionState ConnectionState { get; private set; }

        /// <summary>
        /// Unique connection ID
        /// </summary>
        public string ConnectionId { get; private set; }

        /// <summary>
        /// Is connected and polling
        /// </summary>
        public bool IsConnected 
        { 
            get { return ConnectionState == NotificationConnectionState.Connected && _isPolling; } 
        }

        /// <summary>
        /// Current user AD
        /// </summary>
        public string CurrentUserAd { get { return _userAd; } }

        /// <summary>
        /// Polling interval in milliseconds (default: 3000, min: 1000)
        /// </summary>
        public int PollIntervalMs
        {
            get { return _pollIntervalMs; }
            set { _pollIntervalMs = Math.Max(1000, value); }
        }

        /// <summary>
        /// Max consecutive errors before disconnect (default: 10)
        /// </summary>
        public int MaxConsecutiveErrors
        {
            get { return _maxConsecutiveErrors; }
            set { _maxConsecutiveErrors = Math.Max(3, value); }
        }

        #endregion

        #region Events

        /// <summary>
        /// Fired when new notification received
        /// </summary>
        public event EventHandler<NotificationEventArgs> NotificationReceived;

        /// <summary>
        /// Fired when unread count updated
        /// </summary>
        public event EventHandler<UnreadCountEventArgs> UnreadCountUpdated;

        /// <summary>
        /// Fired when pending notifications loaded
        /// </summary>
        public event EventHandler<PendingNotificationsEventArgs> PendingNotificationsReceived;

        /// <summary>
        /// Fired when connection state changes
        /// </summary>
        public event EventHandler<ConnectionStateEventArgs> ConnectionStateChanged;

        /// <summary>
        /// Fired when error occurs
        /// </summary>
        public event EventHandler<NotificationErrorEventArgs> ErrorOccurred;

        /// <summary>
        /// Fired when successfully connected
        /// </summary>
        public event EventHandler<ConnectedEventArgs> Connected;

        #endregion

        #region Constructor

        /// <summary>
        /// Create notification client
        /// </summary>
        /// <param name="serverUrl">Server URL (e.g., https://150.1.8.29:7157)</param>
        /// <param name="appCode">Application code (e.g., CMS)</param>
        /// <param name="apiKey">API Key</param>
        public NotificationClient(string serverUrl, string appCode, string apiKey)
        {
            if (string.IsNullOrEmpty(serverUrl))
                throw new ArgumentNullException("serverUrl");
            if (string.IsNullOrEmpty(appCode))
                throw new ArgumentNullException("appCode");
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException("apiKey");

            _serverUrl = serverUrl.TrimEnd('/');
            _appCode = appCode;
            _apiKey = apiKey;
            ConnectionState = NotificationConnectionState.Disconnected;
            _lastPollTime = DateTime.MinValue;

            InitializeHttpClient();
        }

        private void InitializeHttpClient()
        {
            // SSL Certificate validation bypass for development
            ServicePointManager.ServerCertificateValidationCallback =
                delegate { return true; };

            // TLS 1.2 for .NET 4.5
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 
                | SecurityProtocolType.Tls11 
                | SecurityProtocolType.Tls;

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(_serverUrl),
                Timeout = TimeSpan.FromSeconds(_connectionTimeoutSec)
            };

            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        #endregion

        #region Connection Methods

        /// <summary>
        /// Connect to notification server and start polling
        /// </summary>
        public async Task ConnectAsync(string userAd, string deviceInfo = null)
        {
            if (string.IsNullOrEmpty(userAd))
                throw new ArgumentNullException("userAd");

            // Prevent multiple connections
            lock (_lockObject)
            {
                if (_isPolling)
                {
                    Log("Already connected, disconnecting first...");
                    DisconnectInternal();
                }
            }

            _userAd = userAd;
            _deviceInfo = deviceInfo ?? Environment.MachineName;
            _lastNotificationId = 0;
            _consecutiveErrors = 0;
            _lastPollTime = DateTime.UtcNow;

            try
            {
                SetConnectionState(NotificationConnectionState.Connecting);
                Log("Connecting to " + _serverUrl);
                Log("UserAd: " + _userAd + ", AppCode: " + _appCode);

                // Test server connection
                if (!await TestServerConnectionAsync())
                {
                    throw new Exception("Cannot connect to notification server");
                }

                Log("Server connection OK");

                // Generate connection ID
                ConnectionId = Guid.NewGuid().ToString("N").Substring(0, 16);

                // Register user connection to server
                await RegisterUserConnectionAsync();

                // Load initial data
                await LoadInitialDataAsync();

                // Start polling
                StartPollingThread();

                SetConnectionState(NotificationConnectionState.Connected);
                Log("Connected successfully! ConnectionId: " + ConnectionId);

                // Fire connected event
                OnConnected(ConnectionId);
            }
            catch (Exception ex)
            {
                Log("Connection failed: " + ex.Message);
                SetConnectionState(NotificationConnectionState.Error);
                OnError(ex, "Connection failed");
                throw;
            }
        }

        /// <summary>
        /// Disconnect from server
        /// </summary>
        public void Disconnect()
        {
            Log("Disconnecting...");
            
            try
            {
                UnregisterUserConnectionAsync().Wait(3000);
            }
            catch { }
            
            DisconnectInternal();
            SetConnectionState(NotificationConnectionState.Disconnected);
            Log("Disconnected");
        }

        /// <summary>
        /// Disconnect async
        /// </summary>
        public async Task DisconnectAsync()
        {
            Log("Disconnecting...");
            
            try
            {
                await UnregisterUserConnectionAsync();
            }
            catch { }
            
            DisconnectInternal();
            SetConnectionState(NotificationConnectionState.Disconnected);
            Log("Disconnected");
        }

        private void DisconnectInternal()
        {
            lock (_lockObject)
            {
                _isPolling = false;

                if (_pollingCts != null)
                {
                    try
                    {
                        _pollingCts.Cancel();
                        _pollingCts.Dispose();
                    }
                    catch { }
                    _pollingCts = null;
                }

                if (_pollingThread != null && _pollingThread.IsAlive)
                {
                    try
                    {
                        _pollingThread.Join(2000);
                        if (_pollingThread.IsAlive)
                            _pollingThread.Abort();
                    }
                    catch { }
                    _pollingThread = null;
                }
            }
        }

        private async Task<bool> TestServerConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Log("Health check failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Register user connection to server
        /// </summary>
        private async Task RegisterUserConnectionAsync()
        {
            try
            {
                var request = new
                {
                    appCode = _appCode,
                    userAd = _userAd,
                    connectionId = ConnectionId,
                    connectionType = "POLLING",
                    deviceInfo = _deviceInfo,
                    connectedAt = DateTime.UtcNow.ToString("o")
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Log("Registering user connection: " + _userAd);
                
                var response = await _httpClient.PostAsync("/api/connections/register", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    Log("User registered successfully: " + responseJson);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Log("Registration endpoint not found (server may not support it)");
                }
                else
                {
                    Log("Registration failed: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Log("Registration error (non-fatal): " + ex.Message);
            }
        }

        /// <summary>
        /// Unregister user connection from server
        /// </summary>
        private async Task UnregisterUserConnectionAsync()
        {
            try
            {
                var url = string.Format("/api/connections/{0}?userAd={1}&connectionId={2}",
                    Uri.EscapeDataString(_appCode),
                    Uri.EscapeDataString(_userAd),
                    Uri.EscapeDataString(ConnectionId ?? ""));

                Log("Unregistering user connection: " + _userAd);
                
                var response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    Log("User unregistered successfully");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Log("Unregister endpoint not found (server may not support it)");
                }
                else
                {
                    Log("Unregister failed: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Log("Unregister error (non-fatal): " + ex.Message);
            }
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                // Get unread count
                var counts = await GetUnreadCountInternalAsync();
                if (counts != null && counts.Count > 0)
                {
                    OnUnreadCountUpdated(counts);
                }

                // Get pending (unread) notifications
                var pending = await GetNotificationsInternalAsync(null, false, 1, 20);
                if (pending != null && pending.Items != null && pending.Items.Count > 0)
                {
                    // Update last ID
                    foreach (var n in pending.Items)
                    {
                        EnrichNotification(n);
                        if (n.NotificationId > _lastNotificationId)
                            _lastNotificationId = n.NotificationId;
                    }

                    OnPendingNotificationsReceived(pending.Items);
                }
            }
            catch (Exception ex)
            {
                Log("LoadInitialData error: " + ex.Message);
            }
        }

        #endregion

        #region Polling

        private void StartPollingThread()
        {
            _pollingCts = new CancellationTokenSource();
            _isPolling = true;
            _heartbeatCounter = 0;

            _pollingThread = new Thread(PollingLoop)
            {
                Name = "NotificationPollingThread",
                IsBackground = true
            };
            _pollingThread.Start();
        }

        private int _heartbeatCounter;
        private const int HEARTBEAT_INTERVAL = 10;

        private void PollingLoop()
        {
            Log("Polling loop started (interval: " + _pollIntervalMs + "ms)");
            var token = _pollingCts.Token;

            while (_isPolling && !token.IsCancellationRequested)
            {
                try
                {
                    // Poll for new notifications
                    PollForNotificationsAsync().Wait(token);
                    _consecutiveErrors = 0;

                    // Send heartbeat periodically
                    _heartbeatCounter++;
                    if (_heartbeatCounter >= HEARTBEAT_INTERVAL)
                    {
                        _heartbeatCounter = 0;
                        SendHeartbeatAsync().Wait(token);
                    }

                    // Reset connection state if was reconnecting
                    if (ConnectionState == NotificationConnectionState.Reconnecting)
                    {
                        RegisterUserConnectionAsync().Wait(token);
                        SetConnectionState(NotificationConnectionState.Connected);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (AggregateException ae)
                {
                    if (ae.InnerException is OperationCanceledException)
                        break;

                    HandlePollingError(ae.InnerException ?? ae);
                }
                catch (Exception ex)
                {
                    HandlePollingError(ex);
                }

                // Wait for next poll
                try
                {
                    Thread.Sleep(_pollIntervalMs);
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
            }

            Log("Polling loop ended");
        }

        private async Task SendHeartbeatAsync()
        {
            try
            {
                var request = new
                {
                    appCode = _appCode,
                    userAd = _userAd,
                    connectionId = ConnectionId,
                    timestamp = DateTime.UtcNow.ToString("o")
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/connections/heartbeat", content);

                if (response.IsSuccessStatusCode)
                {
                    Log("Heartbeat sent");
                }
            }
            catch (Exception ex)
            {
                Log("Heartbeat error (non-fatal): " + ex.Message);
            }
        }

        private void HandlePollingError(Exception ex)
        {
            _consecutiveErrors++;
            Log("Poll error #" + _consecutiveErrors + ": " + ex.Message);

            if (_consecutiveErrors >= _maxConsecutiveErrors)
            {
                Log("Too many consecutive errors, stopping...");
                _isPolling = false;
                SetConnectionState(NotificationConnectionState.Error);
                OnError(ex, "Connection lost after " + _maxConsecutiveErrors + " consecutive errors");
            }
            else
            {
                SetConnectionState(NotificationConnectionState.Reconnecting);

                // Exponential backoff
                var backoffMs = Math.Min(_pollIntervalMs * _consecutiveErrors, _maxBackoffMs);
                Log("Backing off for " + backoffMs + "ms");
                Thread.Sleep(backoffMs);
            }
        }

        private async Task PollForNotificationsAsync()
        {
            // Get unread notifications
            var url = string.Format("/api/notifications/user/{0}?pageNumber=1&pageSize=50&isRead=false",
                Uri.EscapeDataString(_userAd));

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponse<PagedResultNotification<NotificationDto>>>(json);

            if (result != null && result.Success && result.Data != null && result.Data.Items != null)
            {
                // Find new notifications (ID > lastNotificationId)
                var newNotifications = result.Data.Items
                    .Where(n => n.NotificationId > _lastNotificationId)
                    .OrderBy(n => n.NotificationId)
                    .ToList();

                foreach (var notification in newNotifications)
                {
                    EnrichNotification(notification);
                    _lastNotificationId = notification.NotificationId;

                    Log("New notification: " + notification.NotificationId + " - " + notification.Title);
                    OnNotificationReceived(notification);
                }

                // Update unread count
                if (result.Data.TotalCount > 0 || newNotifications.Count > 0)
                {
                    await RefreshUnreadCountAsync();
                }
            }

            _lastPollTime = DateTime.UtcNow;
        }

        private void EnrichNotification(NotificationDto notification)
        {
            if (string.IsNullOrEmpty(notification.TypeNameTH))
                notification.TypeNameTH = NotificationDto.GetTypeNameTH(notification.TypeCode);

            if (string.IsNullOrEmpty(notification.ColorCode))
                notification.ColorCode = NotificationDto.GetColorForType(notification.TypeCode);
        }

        #endregion

        #region Public API Methods

        /// <summary>
        /// Get unread count
        /// </summary>
        public async Task<List<UnreadCountDto>> GetUnreadCountAsync()
        {
            return await GetUnreadCountInternalAsync();
        }

        /// <summary>
        /// Get notifications
        /// </summary>
        public async Task<PagedResultNotification<NotificationDto>> GetNotificationsAsync(
            string typeCode = null,
            bool? isRead = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            return await GetNotificationsInternalAsync(typeCode, isRead, pageNumber, pageSize);
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        public async Task<bool> MarkAsReadAsync(long notificationId)
        {
            try
            {
                var url = string.Format("/api/notifications/{0}/read?userAd={1}",
                    notificationId, Uri.EscapeDataString(_userAd));

                var response = await _httpClient.PutAsync(url, 
                    new StringContent("", Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    await RefreshUnreadCountAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log("MarkAsRead error: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        public async Task<int> MarkAllAsReadAsync(string typeCode = null)
        {
            try
            {
                var url = string.Format("/api/notifications/user/{0}/read-all",
                    Uri.EscapeDataString(_userAd));

                if (!string.IsNullOrEmpty(typeCode))
                    url += "?typeCode=" + Uri.EscapeDataString(typeCode);

                var response = await _httpClient.PutAsync(url,
                    new StringContent("", Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ApiResponse<int>>(json);

                    await RefreshUnreadCountAsync();
                    return result != null ? result.Data : 0;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Log("MarkAllAsRead error: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Delete notification
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(long notificationId)
        {
            try
            {
                var url = string.Format("/api/notifications/{0}?userAd={1}",
                    notificationId, Uri.EscapeDataString(_userAd));

                var response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    await RefreshUnreadCountAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log("DeleteNotification error: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Send notification
        /// </summary>
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
            try
            {
                var request = new
                {
                    typeCode = typeCode,
                    title = title,
                    message = message,
                    recipientUserAd = recipientUserAd,
                    recipientType = "USER",
                    senderUserAd = senderUserAd ?? _userAd,
                    referenceType = referenceType,
                    referenceId = referenceId,
                    referenceNo = referenceCode,
                    priority = priority
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/notifications/send", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<NotificationDto>>(responseJson);

                return result != null ? result.Data : null;
            }
            catch (Exception ex)
            {
                Log("SendNotification error: " + ex.Message);
                OnError(ex, "Failed to send notification");
                return null;
            }
        }

        /// <summary>
        /// Refresh unread count manually
        /// </summary>
        public async Task RefreshUnreadCountAsync()
        {
            try
            {
                var counts = await GetUnreadCountInternalAsync();
                if (counts != null)
                {
                    OnUnreadCountUpdated(counts);
                }
            }
            catch (Exception ex)
            {
                Log("RefreshUnreadCount error: " + ex.Message);
            }
        }

        #endregion

        #region Internal API Methods

        private async Task<List<UnreadCountDto>> GetUnreadCountInternalAsync()
        {
            var url = string.Format("/api/notifications/user/{0}/unread",
                Uri.EscapeDataString(_userAd));

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponse<List<UnreadCountDto>>>(json);

            return result != null && result.Success ? result.Data : new List<UnreadCountDto>();
        }

        private async Task<PagedResultNotification<NotificationDto>> GetNotificationsInternalAsync(
            string typeCode, bool? isRead, int pageNumber, int pageSize)
        {
            var url = string.Format("/api/notifications/user/{0}?pageNumber={1}&pageSize={2}",
                Uri.EscapeDataString(_userAd), pageNumber, pageSize);

            if (!string.IsNullOrEmpty(typeCode))
                url += "&typeCode=" + Uri.EscapeDataString(typeCode);
            if (isRead.HasValue)
                url += "&isRead=" + isRead.Value.ToString().ToLower();

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponse<PagedResultNotification<NotificationDto>>>(json);

            if (result != null && result.Success && result.Data != null)
            {
                foreach (var n in result.Data.Items)
                {
                    EnrichNotification(n);
                }
                return result.Data;
            }

            return new PagedResultNotification<NotificationDto>();
        }

        #endregion

        #region Event Helpers

        private void SetConnectionState(NotificationConnectionState state)
        {
            if (ConnectionState != state)
            {
                var oldState = ConnectionState;
                ConnectionState = state;

                var handler = ConnectionStateChanged;
                if (handler != null)
                {
                    handler(this, new ConnectionStateEventArgs
                    {
                        OldState = oldState,
                        NewState = state
                    });
                }
            }
        }

        private void OnNotificationReceived(NotificationDto notification)
        {
            var handler = NotificationReceived;
            if (handler != null)
            {
                handler(this, new NotificationEventArgs { Notification = notification });
            }
        }

        private void OnUnreadCountUpdated(List<UnreadCountDto> counts)
        {
            var handler = UnreadCountUpdated;
            if (handler != null)
            {
                handler(this, new UnreadCountEventArgs { Counts = counts });
            }
        }

        private void OnPendingNotificationsReceived(List<NotificationDto> notifications)
        {
            var handler = PendingNotificationsReceived;
            if (handler != null)
            {
                handler(this, new PendingNotificationsEventArgs { Notifications = notifications });
            }
        }

        private void OnConnected(string connectionId)
        {
            var handler = Connected;
            if (handler != null)
            {
                handler(this, new ConnectedEventArgs { ConnectionId = connectionId });
            }
        }

        private void OnError(Exception ex, string context)
        {
            var handler = ErrorOccurred;
            if (handler != null)
            {
                handler(this, new NotificationErrorEventArgs
                {
                    Exception = ex,
                    Context = context
                });
            }
        }

        private void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine("[NotificationClient] " + message);
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
                    DisconnectInternal();

                    if (_httpClient != null)
                    {
                        _httpClient.Dispose();
                        _httpClient = null;
                    }
                }
                _disposed = true;
            }
        }

        #endregion
    }

    #region Event Args Classes

    public class NotificationEventArgs : EventArgs
    {
        public NotificationDto Notification { get; set; }
    }

    public class UnreadCountEventArgs : EventArgs
    {
        public List<UnreadCountDto> Counts { get; set; }

        public int TotalCount
        {
            get
            {
                if (Counts == null) return 0;
                var all = Counts.FirstOrDefault(c => c.TypeCode == "ALL");
                return all != null ? all.UnreadCount : Counts.Sum(c => c.UnreadCount);
            }
        }
    }

    public class PendingNotificationsEventArgs : EventArgs
    {
        public List<NotificationDto> Notifications { get; set; }
    }

    public class ConnectionStateEventArgs : EventArgs
    {
        public NotificationConnectionState OldState { get; set; }
        public NotificationConnectionState NewState { get; set; }
    }

    public class ConnectedEventArgs : EventArgs
    {
        public string ConnectionId { get; set; }
    }

    public class NotificationErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public string Context { get; set; }
    }

    #endregion
}
