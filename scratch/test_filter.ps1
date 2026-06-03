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

$tasks = $wfService.GetMyTasks($userContext)

# Helper to map
function Map-TaskItem($result) {
    $dueDate = $result.SLADeadline
    $now = [DateTime]::Now
    $daysLeft = [int]($dueDate - $now).TotalDays
    
    $slaPercent = 0
    if ($result.TimeRemainingMinutes -ne $null) {
        $totalMinutes = ($dueDate - $result.AssignedDate).TotalMinutes
        if ($totalMinutes -gt 0) {
            $elapsedPercent = ($totalMinutes - $result.TimeRemainingMinutes) / $totalMinutes * 100
            $slaPercent = [Math]::Min(100, [int]$elapsedPercent)
        }
    }
    
    # Mapping logic from code:
    # var statusText = result.SLAStatus == SLAStatus.Overdue ? "เกิน SLA"
    #                : result.Status == WorkflowStepInstanceStatus.Pending ? "รอดำเนินการ"
    #                : result.Status == WorkflowStepInstanceStatus.Completed ? "เสร็จ"
    #                : "รอ";
    $statusText = "รอ"
    if ($result.SLAStatus -eq [CMS.Domain.Enums.SLAStatus]::Overdue) {
        $statusText = "เกิน SLA"
    } elseif ($result.Status -eq [CMS.Domain.Enums.WorkflowStepInstanceStatus]::Pending) {
        $statusText = "รอดำเนินการ"
    } elseif ($result.Status -eq [CMS.Domain.Enums.WorkflowStepInstanceStatus]::Completed) {
        $statusText = "เสร็จ"
    }
    
    return [PSCustomObject]@{
        WorkflowId = "WFI-{0:D5}" -f $result.WorkflowInstanceId
        WorkflowTitle = if ($result.WorkflowName) { $result.WorkflowName } else { "ไม่มีชื่อ" }
        Status = $statusText
        DueDate = $dueDate.ToString("dd/MM/yy")
    }
}

$mappedTasks = foreach ($t in $tasks) { Map-TaskItem($t) }

Write-Host "Mapped Tasks:"
$mappedTasks | Format-Table -AutoSize

# Let's apply filters under different index options
# 1. Start filters (all active/all, search="ค้นหา...", statusIndex=0, typeIndex=0)
$filtered = $mappedTasks
$q = "ค้นหา..."
$q_clean = $q.Trim().ToLower()

if ($q_clean -ne "" -and $q_clean -ne "ค้นหา...") {
    $filtered = $filtered | Where-Object { 
        $_.WorkflowId.ToLower().Contains($q_clean) -or 
        $_.WorkflowTitle.ToLower().Contains($q_clean) 
    }
}

Write-Host "Filtered (at startup):"
$filtered | Format-Table -AutoSize
