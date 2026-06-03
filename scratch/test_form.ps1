$binPath = "c:\Users\sa463\OneDrive\Documents\cms_import\CMS.Presentation\bin\Debug"
[System.AppDomain]::CurrentDomain.SetData("APP_CONFIG_FILE", "$binPath\CMS.Presentation.exe.config")
# Reset ConfigManager
[System.Configuration.ConfigurationManager].GetField("s_initState", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static).SetValue($null, 0)
[System.Configuration.ConfigurationManager].GetField("s_configSystem", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static).SetValue($null, $null)
[System.Configuration.ConfigurationManager].GetMethod("PrepareConfigSystem", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static).Invoke($null, $null)

Add-Type -Path "$binPath\CMS.Domain.dll"
Add-Type -Path "$binPath\CMS.Infrastructure.dll"
Add-Type -Path "$binPath\CMS.Data.dll"
Add-Type -Path "$binPath\CMS.Services.dll"
Add-Type -AssemblyName System.Windows.Forms

# Load presentation EXE
[System.Reflection.Assembly]::LoadFrom("$binPath\CMS.Presentation.exe") | Out-Null

$configManager = New-Object CMS.Infrastructure.ConfigManager
$logger = New-Object CMS.Infrastructure.LoggingService

# Repositories & Services
$wfRepo = New-Object CMS.Data.Repositories.WorkflowRepository($configManager)
$wfTempRepo = New-Object CMS.Data.Repositories.WorkflowTemplateRepository($configManager)
$wfStepRepo = New-Object CMS.Data.Repositories.WorkflowStepRepository($configManager)
$wfStepInstRepo = New-Object CMS.Data.Repositories.WorkflowStepInstanceRepository($configManager)
$delegationRepo = New-Object CMS.Data.Repositories.DelegationRepository($configManager)
$wfReportRepo = New-Object CMS.Data.Repositories.WFReportRepository($configManager)
$auditRepo = New-Object CMS.Data.Repositories.AuditRepository($configManager)
$auditService = New-Object CMS.Services.AuditService($auditRepo, $logger)
$wfNotifService = New-Object CMS.Services.WorkflowNotificationService($auditService, $logger)
$slaCalc = New-Object CMS.Services.SLACalculator($wfStepInstRepo, $wfReportRepo, $wfNotifService, $auditService, $logger)
$condEval = New-Object CMS.Services.ConditionEvaluatorService($logger)
$stepResolver = New-Object CMS.Services.StepResolverService($wfStepRepo, $condEval, $logger)
$wfService = New-Object CMS.Services.WorkflowService($wfRepo, $wfTempRepo, $wfStepRepo, $wfStepInstRepo, $stepResolver, $slaCalc, $wfNotifService, $auditService, $logger)

$userContext = [System.Activator]::CreateInstance([CMS.Domain.Models.UserContext])
[CMS.Domain.Models.UserContext].GetProperty("ADUserName").SetValue($userContext, "pitakpong")
[CMS.Domain.Models.UserContext].GetProperty("UserID").SetValue($userContext, [int]1)

# Instantiate the form using the constructor
$form = New-Object CMS.Presentation.Modules.Workflow.WorkflowDashboardFormNew(
    $wfService,
    $slaCalc,
    $logger,
    $userContext,
    $null,
    $null
)

# Trigger Load event programmatically by calling OnLoad
$onLoadMethod = [System.Windows.Forms.Form].GetMethod("OnLoad", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
$onLoadMethod.Invoke($form, @([System.EventArgs]::Empty))

# Inspect private state of the form using reflection
$type = $form.GetType()

# 1. Inspect _allTasks
$allTasksField = $type.GetField("_allTasks", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
$allTasks = $allTasksField.GetValue($form)
Write-Host "Form _allTasks Count: $($allTasks.Count)"

# 2. Inspect txtSearch text
$txtSearchField = $type.GetField("txtSearch", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
$txtSearch = $txtSearchField.GetValue($form)
Write-Host "Form txtSearch.Text: '$($txtSearch.Text)'"

# 3. Inspect cmbFilterStatus SelectedIndex
$cmbFilterStatusField = $type.GetField("cmbFilterStatus", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
$cmbFilterStatus = $cmbFilterStatusField.GetValue($form)
Write-Host "Form cmbFilterStatus SelectedIndex: $($cmbFilterStatus.SelectedIndex)"

# 4. Inspect cmbFilterType SelectedIndex
$cmbFilterTypeField = $type.GetField("cmbFilterType", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
$cmbFilterType = $cmbFilterTypeField.GetValue($form)
Write-Host "Form cmbFilterType SelectedIndex: $($cmbFilterType.SelectedIndex)"

# 5. Inspect lvTasks Items Count
$lvTasksField = $type.GetField("lvTasks", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
$lvTasks = $lvTasksField.GetValue($form)
Write-Host "Form lvTasks Items Count: $($lvTasks.Items.Count)"

for ($i = 0; $i -lt $lvTasks.Items.Count; $i++) {
    $item = $lvTasks.Items[$i]
    Write-Host "  Item $i Tag WorkflowId: $($item.Tag.WorkflowId)"
    Write-Host "  Item $i Tag Status: $($item.Tag.Status)"
}

# Clean up
$form.Dispose()
