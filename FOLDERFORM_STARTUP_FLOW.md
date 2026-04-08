# FolderForm Startup Flow - เปิด Form ทีแรกเมื่อเริ่มโปรแกรม

## 📊 Sequence Diagram: FolderForm Initialization

```
┌─────────────┐
│   Program   │
└──────┬──────┘
       │
       │ Main()
       │
       ▼
┌──────────────────────────────────────┐
│ Application Startup                  │
│ • Load dependencies (DI Container)   │
│ • Create MDI_Container instance      │
└──────┬───────────────────────────────┘
       │
       │ MDI_Container Form created
       │
       ▼
┌──────────────────────────────────────┐
│ MDI_Container.Load() Event           │
│ (fires before form shows)            │
└──────┬───────────────────────────────┘
       │
       │ ✓ Set fullscreen mode
       │ ✓ Initialize toolbar
       │ ✓ Setup menus
       │ ✓ Wire event handlers
       │ ✓ Build dashboard
       │ ✓ Start timers
       │
       │ ✓ All UI components ready
       │
       ▼
┌──────────────────────────────────────┐
│ this.BeginInvoke()                   │
│ (defer to UI thread after render)    │
└──────┬───────────────────────────────┘
       │
       │ UI thread executes deferred action
       │
       ▼
┌──────────────────────────────────────┐
│ OpenFolderStructure()                │
└──────┬───────────────────────────────┘
       │
       │ ✓ Validate _folderService
       │ ✓ Validate _fileService
       │ ✓ Validate _folderRepository
       │
       ├─────────────────────────────┐
       │                             │
   FAIL (❌)                     SUCCESS (✓)
       │                             │
       ▼                             ▼
   Show Error              new FolderForm(...)
   & Exit                          │
                                   ├─ Constructor validation
                                   ├─ Initialize components
                                   ├─ Load folder tree
                                   │
                                   ▼
                          Wire Event Handlers
                          • OpenPermissionRequested
                          • OpenFilePermissionRequested
                          • SelectionChanged
                                   │
                                   ▼
                          OpenChildForm(ff)
                          • Set MdiParent
                          • Show()
                          • Activate
                                   │
                                   ▼
                          ✅ FolderForm visible
                          & ready for interaction
```

---

## 🔄 Code Flow

### 1️⃣ Program Start

```csharp
static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        // Setup DI Container
        var services = ConfigureServices();
        var mdiContainer = services.GetRequiredService<MDI_Container>();
        
        Application.Run(mdiContainer);  // ← Form shows here
    }
}
```

### 2️⃣ MDI_Container Form Created

```csharp
public MDI_Container(
    IFolderService folderService,
    IFileService fileService,
    IFolderRepository folderRepository,
    // ... other dependencies
)
{
    InitializeComponent();  // ← Designer-generated code
    
    // Store dependencies
    _folderService = folderService;
    _fileService = fileService;
    _folderRepository = folderRepository;
    // ...
}
// ← Load event fires AFTER InitializeComponent but BEFORE Show()
```

### 3️⃣ MDI_Container_Load Event (NEW IMPROVED VERSION)

```csharp
private void MDI_Container_Load(object sender, EventArgs e)
{
    try
    {
        // Step 1: Setup UI components
        this.WindowState = FormWindowState.Maximized;
        Text = $"CMS — {_currentUser.AdIdentity}";
        BuildGroupedAdminToolbar();
        ApplyRoleBasedMenuVisibility();
        WireRibbonHandlers();
        WireDashboardHandlers();
        SetupRecentFilesColumns();
        LoadDashboardFolderTree();
        LoadRecentFiles();
        UpdateStatusBar();
        LoadDashboardStats();
        _escalationTimer.Start();
        
        // ... more UI setup ...
        
        // Step 2: Wire event handlers
        this.MdiChildActivate += (s, ev) => OnMdiChildActivate();
        
        // Step 3: DEFER FolderForm opening to UI thread
        // This ensures form is fully rendered before opening MDI child
        this.BeginInvoke(new Action(() =>
        {
            try
            {
                OpenFolderStructure();
                _logger.LogInfo("FolderForm opened successfully on startup.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error opening FolderForm: {ex.Message}");
                MessageBox.Show($"Error opening Folder Manager: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }));
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error during MDI_Container initialization: {ex.Message}");
        MessageBox.Show($"Initialization error: {ex.Message}", "Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

### 4️⃣ OpenFolderStructure() Method (IMPROVED)

```csharp
private void OpenFolderStructure()
{
    // Validate all required services
    if (_folderService == null)
        throw new InvalidOperationException("FolderService not available");
    
    if (_fileService == null)
        throw new InvalidOperationException("FileService not available");
    
    if (_folderRepository == null)
        throw new InvalidOperationException("FolderRepository not available");
    
    try
    {
        // Create FolderForm instance with ALL dependencies
        var ff = new FolderForm(
            _folderService,              // Required
            _currentUser,                // Required
            null,                        // OrgUnitId = null (show all)
            _fileService,                // Required
            _orgStructureService,        // Optional but recommended
            _folderTemplateRepository,   // Optional
            _uploadCheckpointService,    // Optional
            _quickAccessService,         // Optional
            _approvalService,            // Optional
            _permissionService,          // Optional
            _folderRepository,           // Required
            _permissionRepository,       // Optional
            _auditService,               // Optional
            _shareService,               // Optional
            _userAccountRepository       // Optional
        );
        
        // Wire event handlers for communication
        ff.OpenPermissionRequested     += (s, folder) => OpenFolderSettingsForFolder(folder);
        ff.OpenFilePermissionRequested += (s, file)   => OpenFilePermissionDialog(file);
        ff.SelectionChanged            += (s, ev)     => UpdateMoveControlsState();
        
        // Open as MDI child
        OpenChildForm(ff);
        
        _logger.LogInfo("FolderForm (MOD-002) opened successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error creating FolderForm: {ex.Message}");
        throw;  // Re-throw for caller to handle
    }
}
```

### 5️⃣ OpenChildForm() Helper

```csharp
private void OpenChildForm(Form childForm)
{
    if (childForm == null)
        throw new ArgumentNullException(nameof(childForm));
    
    try
    {
        childForm.MdiParent = this;  // Set parent to MDI container
        childForm.Show();             // Show the form
        childForm.Activate();         // Bring to front
        
        // Auto-hide dashboard when MDI child is open
        pnlDashboard.Visible = false;
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error opening child form: {ex.Message}");
        throw;
    }
}
```

---

## 📝 Timeline: What Happens When

| Time | What Happens | State |
|------|--------------|-------|
| T0 | Program starts | Application initializes |
| T1 | DI Container setup | Services registered |
| T2 | MDI_Container created | Constructor runs |
| T3 | MDI_Container.Load fires | UI components setup |
| T4 | Form renders to screen | User sees MDI container |
| T5 | BeginInvoke executes | OpenFolderStructure() called |
| T6 | FolderForm created | Constructor validation |
| T7 | Event handlers wired | Communication ready |
| T8 | OpenChildForm() called | FolderForm becomes MDI child |
| T9 | FolderForm.Show() | ✅ **User sees FolderForm** |

---

## 🎯 Key Points

### ✅ Why Use BeginInvoke?

```csharp
// ❌ WRONG: Direct call
OpenFolderStructure();  // May open before UI ready

// ✅ CORRECT: Deferred call
this.BeginInvoke(new Action(() => OpenFolderStructure()));
// Opens only after UI thread is ready
```

### ✅ Why Validate Dependencies?

```csharp
// ✓ Catches missing dependencies early
// ✓ Provides clear error messages
// ✓ Prevents NullReferenceException deep in form loading
// ✓ Makes debugging easier

if (_folderService == null)
    throw new InvalidOperationException("FolderService not available");
```

### ✅ Why Wire Event Handlers?

```csharp
// Allows FolderForm to communicate back to MDI_Container
ff.OpenPermissionRequested += (s, folder) => OpenFolderSettingsForFolder(folder);
ff.OpenFilePermissionRequested += (s, file) => OpenFilePermissionDialog(file);
ff.SelectionChanged += (s, ev) => UpdateMoveControlsState();
```

---

## 🔴 Troubleshooting

### Problem: FolderForm doesn't open on startup

**Cause**: Dependencies are null

**Solution**:
```csharp
// Check DI container registration
services.AddScoped<IFolderService, FolderService>();  // ✓ Must be registered
services.AddScoped<IFileService, FileService>();      // ✓ Must be registered
services.AddScoped<IFolderRepository, FolderRepository>(); // ✓ Must be registered
```

### Problem: "NullReferenceException" when FolderForm opens

**Cause**: Service not initialized

**Solution**:
```csharp
// Add logging to see which service is null
_logger.LogInfo($"FolderService: {(_folderService != null ? "OK" : "NULL")}");
_logger.LogInfo($"FileService: {(_fileService != null ? "OK" : "NULL")}");
_logger.LogInfo($"FolderRepository: {(_folderRepository != null ? "OK" : "NULL")}");
```

### Problem: Form flickers before FolderForm opens

**Cause**: Opening too early (before UI ready)

**Solution**: Already fixed by using `BeginInvoke()`

---

## 📋 Requirements Satisfied

| Req # | Description | Status |
|-------|-------------|--------|
| 9.1 | FolderForm opens automatically on startup | ✅ |
| 9.2 | All dependencies validated | ✅ |
| 9.3 | Event handlers wired for communication | ✅ |
| 9.4 | Dashboard hidden when FolderForm active | ✅ |
| 9.5 | Error handling for missing services | ✅ |

---

## 💡 Example: How User Interaction Works

```
1. Program starts
   ↓
2. MDI_Container opens with dashboard
   ↓
3. BeginInvoke triggers
   ↓
4. FolderForm opens as MDI child
   ↓
5. User sees FolderForm with folder tree
   ↓
6. User double-clicks folder
   ↓
7. FolderForm event fires: SelectionChanged
   ↓
8. MDI_Container updates toolbar state
   ↓
9. User can now perform folder operations
```

---

## ✅ Verification Checklist

- [x] MDI_Container_Load has try-catch
- [x] OpenFolderStructure calls BeginInvoke
- [x] All dependencies validated
- [x] Event handlers wired
- [x] Error messages informative
- [x] Logging integrated
- [x] FolderForm opens automatically

**Status**: ✅ Ready for production
