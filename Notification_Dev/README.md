# E-Filing Notification System
## Production Ready for .NET Framework 4.5

ระบบแจ้งเตือนแบบ Real-time* สำหรับ E-Filing Windows Application

> *ใช้ Long Polling (poll ทุก 3 วินาที) แทน SignalR เนื่องจาก .NET Framework 4.5 ไม่รองรับ ASP.NET Core SignalR Client

---

## 📦 ไฟล์ที่รวมอยู่

```
├── Notification/
│   ├── Models/
│   │   └── NotificationDto.cs           # Data models
│   └── Services/
│       ├── NotificationClient.cs        # Long Polling Client
│       └── NotificationManager.cs       # Singleton Manager
├── Forms/
│   └── NotificationAlertPopup.cs        # Popup Form
├── UserControls/
│   ├── NotificationItemControl.cs       # Notification Item UI
│   └── NotificationItemControl.Designer.cs
├── MainForm_NotificationIntegration.cs  # ตัวอย่างการใช้งานใน MainForm
└── README.md
```

---

## 🚀 การติดตั้ง

### Step 1: NuGet Package

```
Install-Package Newtonsoft.Json
```

**ไม่ต้องติดตั้ง SignalR package ใดๆ!**

### Step 2: Copy Files

Copy โฟลเดอร์เหล่านี้ไปไว้ใน E-Filing Project:
- `Notification/` (ทั้งโฟลเดอร์)
- `Forms/NotificationAlertPopup.cs`
- `UserControls/` (ทั้งโฟลเดอร์)

### Step 3: App.config

เพิ่มใน `<appSettings>`:

```xml
<appSettings>
    <!-- Notification Settings -->
    <add key="NotificationServerUrl" value="https://150.1.8.29:7157" />
    <add key="NotificationAppCode" value="CMS" />
    <add key="NotificationApiKey" value="im_DB6037E2-BFAE-488E-AE24-2EA83C7309D8" />
</appSettings>
```

### Step 4: แก้ไข MainForm.cs

#### 4.1 เพิ่ม using statements

```csharp
using E_Filing.Notification.Models;
using E_Filing.Notification.Services;
```

#### 4.2 เพิ่ม Fields และ Configuration

```csharp
// Notification Configuration
private readonly string NOTIFICATION_SERVER_URL = 
    ConfigurationManager.AppSettings["NotificationServerUrl"] ?? "https://150.1.8.29:7157";
private readonly string NOTIFICATION_APP_CODE = 
    ConfigurationManager.AppSettings["NotificationAppCode"] ?? "CMS";
private readonly string NOTIFICATION_API_KEY = 
    ConfigurationManager.AppSettings["NotificationApiKey"] ?? "im_DB6037E2-BFAE-488E-AE24-2EA83C7309D8";

private bool _notificationInitialized = false;
```

#### 4.3 เพิ่ม Initialization Method

```csharp
private async Task InitializeNotificationSystemAsync(string userAd)
{
    if (_notificationInitialized || string.IsNullOrEmpty(userAd)) return;

    try
    {
        // Initialize
        NotificationManager.Instance.Initialize(
            this,
            NOTIFICATION_SERVER_URL,
            NOTIFICATION_APP_CODE,
            NOTIFICATION_API_KEY
        );

        // Events
        NotificationManager.Instance.NotificationReceived += OnNotificationReceived;
        NotificationManager.Instance.UnreadCountChanged += OnUnreadCountChanged;
        NotificationManager.Instance.NotificationClicked += OnNotificationClicked;
        NotificationManager.Instance.ConnectionStateChanged += OnConnectionStateChanged;

        // Connect
        await NotificationManager.Instance.ConnectAsync(userAd);

        _notificationInitialized = true;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine("Notification error: " + ex.Message);
    }
}
```

#### 4.4 เพิ่ม Event Handlers

```csharp
private void OnNotificationReceived(object sender, NotificationDto notification)
{
    // Popup แสดงอัตโนมัติ
    System.Diagnostics.Debug.WriteLine("Notification: " + notification.Title);
}

private void OnUnreadCountChanged(object sender, int count)
{
    // Update badge
    if (this.InvokeRequired)
        this.BeginInvoke(new Action(() => UpdateBadge(count)));
    else
        UpdateBadge(count);
}

private void OnNotificationClicked(object sender, NotificationDto notification)
{
    // Navigate ไปหน้าที่เกี่ยวข้อง
    HandleNotificationNavigation(notification);
}

private void OnConnectionStateChanged(object sender, NotificationConnectionState state)
{
    // Update UI status
}

private void UpdateBadge(int count)
{
    // Update badge label
}

private void HandleNotificationNavigation(NotificationDto notification)
{
    switch (notification.ReferenceType?.ToLower())
    {
        case "claim":
            // Open claim form
            break;
        case "document":
            // Open document form
            break;
    }
}
```

#### 4.5 เรียกใน GetMenuControl

```csharp
private async Task GetMenuControl(string accessToken, int programId)
{
    // ... existing code ...

    if (loginResult != null && loginResult.EmployeeInfo != null)
    {
        // ... set user info ...
        _samAccountName = loginResult.UserName;

        // ===== เพิ่มบรรทัดนี้ =====
        await InitializeNotificationSystemAsync(_samAccountName);
        // ==========================
    }
}
```

---

## 📤 การส่ง Notification

### จาก E-Filing Application

```csharp
// ส่ง Notification มอบหมายงาน
await NotificationManager.Instance.SendNotificationAsync(
    typeCode: "ASSIGN_NOTE",
    title: "คุณได้รับมอบหมายงาน: CLM-2024-001234",
    message: "สมศรี ได้มอบหมายงานเคลมให้คุณดำเนินการ",
    recipientUserAd: "somchai_k",
    senderUserAd: MainForm._samAccountName,
    referenceType: "claim",
    referenceId: "12345",
    referenceCode: "CLM-2024-001234",
    priority: 2
);
```

### จาก Postman

```
POST https://localhost:7157/api/notifications/send
Headers:
  X-API-Key: ef_4FE36BA8-D3C7-4C00-A41A-E1648E567B30
  Content-Type: application/json

Body:
{
    "typeCode": "ASSIGN_NOTE",
    "title": "คุณได้รับมอบหมายงานใหม่",
    "message": "สมศรี ได้มอบหมายงานเคลม CLM-2024-001234 ให้คุณดำเนินการ",
    "recipientUserAd": "sasithorn",
    "senderUserAd": "somsri",
    "referenceType": "claim",
    "referenceId": "12345",
    "referenceNo": "CLM-2024-001234",
    "priority": 2
}
```

**⚠️ สำคัญ:** `recipientUserAd` ต้องตรงกับ username ที่ login ใน E-Filing

---

## 🔔 Notification Types

| Type Code | ชื่อไทย | Icon | สี |
|-----------|---------|------|-----|
| ASSIGN_NOTE | มอบหมายงาน | 📋 | #2196F3 |
| COMMENT | ความคิดเห็น | 💬 | #9C27B0 |
| APPROVAL_DOCUMENT_SUBMIT | ขออนุมัติเอกสาร | 📤 | #FF9800 |
| APPROVAL_DOCUMENT_BORROW | ขอยืมเอกสาร | 📚 | #00BCD4 |
| DEADLINE_REMINDER | แจ้งเตือนกำหนด | ⏰ | #F44336 |
| SYSTEM_ALERT | แจ้งเตือนระบบ | ⚠️ | #FF5722 |
| TRANSFER | โอนย้ายเอกสาร | 📁 | #607D8B |

---

## 🎯 Popup Behavior

| Action | พฤติกรรม |
|--------|----------|
| แสดง | Slide-in จากขวา + เสียงแจ้งเตือน |
| 5 วินาที | Auto fade-out และปิด |
| Mouse hover | หยุด auto-close |
| Mouse leave | เริ่มนับ 5 วินาทีใหม่ |
| Click notification | เปิดหน้าที่เกี่ยวข้อง + ปิด popup |
| Click ✓ | Mark as read |
| Click ✕ | ปิด popup |

---

## 🔍 Debug

ดู **Output Window** ใน Visual Studio:

```
[NotificationClient] Connecting to https://localhost:7157
[NotificationClient] UserAd: sasithorn, AppCode: E-FILING
[NotificationClient] Server connection OK
[NotificationClient] Connected successfully! ConnectionId: abc123def456
[NotificationClient] Polling loop started (interval: 3000ms)
[NotificationClient] New notification: 1 - คุณได้รับมอบหมายงานใหม่
[NotificationManager] Notification received: 1
[NotificationManager] Popup shown: 1
```

---

## ⚙️ Configuration Options

```csharp
// Polling interval (default: 3000ms)
NotificationManager.Instance.PollIntervalMs = 5000; // 5 วินาที

// ปิดเสียง
NotificationManager.Instance.PlaySound = false;

// ปิด Popup (ยังรับ notification แต่ไม่แสดง popup)
NotificationManager.Instance.ShowPopups = false;
```

---

## 📊 เปรียบเทียบกับ SignalR

| Feature | Long Polling (นี้) | SignalR |
|---------|-------------------|---------|
| .NET Framework 4.5 | ✅ รองรับ | ❌ ไม่รองรับ |
| Real-time | ~3 วินาที | <100ms |
| Package Required | Newtonsoft.Json | ไม่ compatible |
| Complexity | ต่ำ | - |
| Reliability | สูง | - |

---

## 🐛 Troubleshooting

### Popup ไม่แสดง

1. ตรวจสอบ `ShowPopups = true`
2. ตรวจสอบว่าเรียก `Initialize()` และ `ConnectAsync()` แล้ว
3. ตรวจสอบ `recipientUserAd` ตรงกับ user ที่ login
4. ดู Output Window สำหรับ error

### Connection ไม่ได้

1. ตรวจสอบ Server URL
2. ตรวจสอบ API Key
3. ตรวจสอบ Server running อยู่ (`/health` endpoint)

### ไม่ได้ยินเสียง

1. ตรวจสอบ `PlaySound = true`
2. ตรวจสอบ volume ของ Windows

---

## 📄 License

Internal use only - E-Filing System
