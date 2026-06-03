$binPath = "c:\Users\sa463\OneDrive\Documents\cms_import\CMS.Presentation\bin\Debug"

# Load the config file for the app domain
[System.AppDomain]::CurrentDomain.SetData("APP_CONFIG_FILE", "$binPath\CMS.Presentation.exe.config")
# Reset ConfigurationManager to pick up the new config file path
[System.Configuration.ConfigurationManager].GetField("s_initState", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static).SetValue($null, 0)
[System.Configuration.ConfigurationManager].GetField("s_configSystem", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static).SetValue($null, $null)
[System.Configuration.ConfigurationManager].GetMethod("PrepareConfigSystem", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static).Invoke($null, $null)

# Load the DLLs
Add-Type -Path "$binPath\CMS.Domain.dll"
Add-Type -Path "$binPath\CMS.Infrastructure.dll"
Add-Type -Path "$binPath\CMS.Data.dll"
Add-Type -Path "$binPath\CMS.Services.dll"

# Instantiate dependencies
$configManager = New-Object CMS.Infrastructure.ConfigManager
$logger = New-Object CMS.Infrastructure.LoggingService

# Repositories
$wfRepo = New-Object CMS.Data.Repositories.WorkflowRepository($configManager)
$wfTempRepo = New-Object CMS.Data.Repositories.WorkflowTemplateRepository($configManager)
$wfStepRepo = New-Object CMS.Data.Repositories.WorkflowStepRepository($configManager)
$wfStepInstRepo = New-Object CMS.Data.Repositories.WorkflowStepInstanceRepository($configManager)
$delegationRepo = New-Object CMS.Data.Repositories.DelegationRepository($configManager)
$wfReportRepo = New-Object CMS.Data.Repositories.WFReportRepository($configManager)

# Services
$auditRepo = New-Object CMS.Data.Repositories.AuditRepository($configManager)
$auditService = New-Object CMS.Services.AuditService($auditRepo, $logger)
$notifRepo = New-Object CMS.Data.Repositories.NotificationRepository($configManager)
$wfNotifService = New-Object CMS.Services.WorkflowNotificationService($auditService, $logger)
$slaCalc = New-Object CMS.Services.SLACalculator($wfStepInstRepo, $wfReportRepo, $wfNotifService, $auditService, $logger)
$condEval = New-Object CMS.Services.ConditionEvaluatorService($logger)
$stepResolver = New-Object CMS.Services.StepResolverService($wfStepRepo, $condEval, $logger)

$wfService = New-Object CMS.Services.WorkflowService(
    $wfRepo,
    $wfTempRepo,
    $wfStepRepo,
    $wfStepInstRepo,
    $stepResolver,
    $slaCalc,
    $wfNotifService,
    $auditService,
    $logger
)

# Create UserContext using reflection or C# type to bypass CLS compliance check
$userContext = [System.Activator]::CreateInstance([CMS.Domain.Models.UserContext])
# set ADUserName
$adUserProp = [CMS.Domain.Models.UserContext].GetProperty("ADUserName")
$adUserProp.SetValue($userContext, "pitakpong")
# set UserID
$userIdProp = [CMS.Domain.Models.UserContext].GetProperty("UserID")
$userIdProp.SetValue($userContext, [int]1)

# Call GetMyTasks
try {
    $tasks = $wfService.GetMyTasks($userContext)
    Write-Host "Service Call Successful! Returned $($tasks.Count) tasks."
    foreach ($t in $tasks) {
        Write-Host "Task Details:"
        Write-Host "  WorkflowStepInstanceId: $($t.WorkflowStepInstanceId)"
        Write-Host "  WorkflowInstanceId: $($t.WorkflowInstanceId)"
        Write-Host "  WorkflowName: $($t.WorkflowName)"
        Write-Host "  StepName: $($t.StepName)"
        Write-Host "  Status: $($t.Status)"
        Write-Host "  InitiatedBy: $($t.InitiatedBy)"
        Write-Host "  SLADeadline: $($t.SLADeadline)"
    }
} catch {
    Write-Host "Error in Service: $($_.Exception.Message)"
    if ($_.Exception.InnerException) {
        Write-Host "Inner: $($_.Exception.InnerException.Message)"
    }
    Write-Host $_.Exception.StackTrace
}
