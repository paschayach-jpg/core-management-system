$connectionString = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $conn.Open()
    
    # 1. Load DB steps
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT WFStepId, StepName FROM WFE.s_WFStep WHERE WFDefinitionId = 9"
    $r = $cmd.ExecuteReader()
    $dbSteps = New-Object System.Collections.Generic.List[PSObject]
    while ($r.Read()) {
        $dbSteps.Add([PSCustomObject]@{
            WorkflowStepId = [int]$r["WFStepId"]
            StepName = [string]$r["StepName"]
        })
    }
    $r.Close()
    
    # 2. Load JSON template definition
    $cmd.CommandText = "SELECT TemplateDefinition FROM WFE.s_WFDefinition WHERE WFDefinitionId = 9"
    $json = $cmd.ExecuteScalar()
    $designerDef = ConvertFrom-Json $json
    
    # 3. Simulate mapping logic
    Write-Output "--- Simulating mapping logic ---"
    $designToSqlMap = @{}
    
    foreach ($s in $designerDef.Steps) {
        # Exact logic from WorkflowVisualFlowForm.cs:
        $matchedDbStep = $null
        foreach ($db in $dbSteps) {
            if ($db.StepName.Equals($s.StepName, [System.StringComparison]::OrdinalIgnoreCase)) {
                $matchedDbStep = $db
                break
            }
        }
        
        $mappedId = $s.WorkflowStepId
        if ($matchedDbStep -ne $null) {
            $mappedId = $matchedDbStep.WorkflowStepId
        }
        $designToSqlMap[$s.WorkflowStepId] = $mappedId
        
        $matchedNameStr = "NULL"
        if ($matchedDbStep -ne $null) {
            $matchedNameStr = "'" + $matchedDbStep.StepName + "' (ID: " + $matchedDbStep.WorkflowStepId + ")"
        }
        Write-Output ("Designer StepName: '" + $s.StepName + "' (ID: " + $s.WorkflowStepId + ") -> Mapped ID: " + $mappedId + " | Matched DB StepName: " + $matchedNameStr)
    }
    
    # 4. Load Step Instances
    Write-Output "`n--- Step Instances in Database ---"
    $cmd.CommandText = "SELECT WFStepInstanceId, WFStepId, Status FROM WFE.t_WFStepInstance WHERE WFInstanceId = 34 AND IsDeleted = 0"
    $r = $cmd.ExecuteReader()
    $stepInstances = New-Object System.Collections.Generic.List[PSObject]
    while ($r.Read()) {
        $stepInstances.Add([PSCustomObject]@{
            WFStepInstanceId = [int]$r["WFStepInstanceId"]
            WFStepId = [int]$r["WFStepId"]
            Status = [int]$r["Status"]
        })
        Write-Output ("StepInstanceId: " + $r["WFStepInstanceId"] + " | StepId: " + $r["WFStepId"] + " | Status: " + $r["Status"])
    }
    $r.Close()
    
    # 5. Check if DrawLiveFlow would find the instance
    Write-Output "`n--- Simulating DrawLiveFlow matching ---"
    foreach ($s in $designerDef.Steps) {
        $mappedId = $designToSqlMap[$s.WorkflowStepId]
        
        # In DrawLiveFlow:
        $stepInstance = $null
        foreach ($si in $stepInstances) {
            if ($si.WFStepId -eq $mappedId) {
                $stepInstance = $si
                break
            }
        }
        
        if ($stepInstance -ne $null) {
            Write-Output ("Step '" + $s.StepName + "' (Mapped ID: " + $mappedId + ") matches StepInstance " + $stepInstance.WFStepInstanceId + " with Status " + $stepInstance.Status)
        } else {
            Write-Output ("Step '" + $s.StepName + "' (Mapped ID: " + $mappedId + ") has NO MATCHING StepInstance!")
        }
    }
    
} catch {
    Write-Error $_.Exception.Message
} finally {
    $conn.Close()
}
