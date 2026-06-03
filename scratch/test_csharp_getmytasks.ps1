try {
    $connStr = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
    
    # 1. Get step instances
    $stepInstances = New-Object System.Collections.Generic.List[PSObject]
    $conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT WFStepInstanceId, WFInstanceId, WFStepId, AssignedTo, AssignedDate, Status, CompletedDate, CompletedBy, OutputData, SLADeadline, SLAStatus, IsDeleted FROM WFE.t_WFStepInstance WHERE AssignedTo = @AssignedTo AND Status IN (1, 2, 3) AND IsDeleted = 0 ORDER BY SLADeadline ASC"
    $cmd.Parameters.AddWithValue("@AssignedTo", "pitakpong")
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        # Emulate GetValue helper
        $GetValue = {
            param($keys, $type)
            foreach ($k in $keys) {
                try {
                    $val = $reader.GetValue($reader.GetOrdinal($k))
                    if ($val -eq [DBNull]::Value) { return $null }
                    return $val
                } catch {
                    # continue
                }
            }
            return $null
        }

        $stepInstance = [PSCustomObject]@{
            WorkflowStepInstanceId = & $GetValue @("WorkflowStepInstanceId", "WFStepInstanceId", "WFStepInstanceID")
            WorkflowInstanceId = & $GetValue @("WorkflowInstanceId", "WFInstanceId", "WorkflowInstanceID")
            WorkflowStepId = & $GetValue @("WorkflowStepId", "WFStepId", "WorkflowStepID")
            AssignedTo = & $GetValue @("AssignedTo", "AssignedUser", "Assigned")
            AssignedDate = & $GetValue @("AssignedDate", "AssignedOn", "StartedDate")
            Status = & $GetValue @("Status")
            CompletedDate = & $GetValue @("CompletedDate")
            CompletedBy = & $GetValue @("CompletedBy")
            OutputData = & $GetValue @("OutputData")
            SLADeadline = & $GetValue @("SLADeadline")
            SLAStatus = & $GetValue @("SLAStatus")
            IsDeleted = & $GetValue @("IsDeleted")
        }
        $stepInstances.Add($stepInstance)
    }
    $reader.Close()
    $conn.Close()

    Write-Host "Mapped step instances:" ($stepInstances | ConvertTo-Json)

    # 2. Get workflow instances
    foreach ($stepInstance in $stepInstances) {
        $conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = "SELECT WFInstanceId as WorkflowInstanceId, WFDefinitionId as WorkflowDefinitionId, TemplateVersion, InitiatedBy, InitiatedDate, CurrentStepId, Status, CompletedDate, InputParameters, IsDeleted, DeletedDate FROM WFE.t_WFInstance WHERE WFInstanceId = @WorkflowInstanceId AND IsDeleted = 0"
        $cmd.Parameters.AddWithValue("@WorkflowInstanceId", $stepInstance.WorkflowInstanceId)
        $reader = $cmd.ExecuteReader()
        if ($reader.Read()) {
            $instance = [PSCustomObject]@{
                WorkflowInstanceId = $reader["WorkflowInstanceId"]
                WorkflowDefinitionId = $reader["WorkflowDefinitionId"]
                TemplateVersion = $reader["TemplateVersion"]
                InitiatedBy = $reader["InitiatedBy"]
                InitiatedDate = $reader["InitiatedDate"]
                CurrentStepId = $reader["CurrentStepId"]
                Status = $reader["Status"]
                CompletedDate = $reader["CompletedDate"]
                InputParameters = $reader["InputParameters"]
                IsDeleted = $reader["IsDeleted"]
                DeletedDate = $reader["DeletedDate"]
            }
            Write-Host "Mapped WorkflowInstance:" ($instance | ConvertTo-Json)
        } else {
            Write-Host "WorkflowInstance not found for ID:" $stepInstance.WorkflowInstanceId
        }
        $reader.Close()
        $conn.Close()
    }
} catch {
    Write-Error $_.Exception.Message
}
