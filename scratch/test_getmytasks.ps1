$connString = 'Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;'
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
try {
    $conn.Open()
    Write-Host 'Connection Successful!'
    
    $username = 'pitakpong'
    Write-Host "Running test for user: $username"
    
    # 1. Get step instances by user
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "
        SELECT WFStepInstanceId, WFInstanceId, WFStepId, 
               AssignedTo, AssignedDate, Status, CompletedDate, CompletedBy, 
               OutputData, SLADeadline, SLAStatus, IsDeleted 
        FROM   WFE.t_WFStepInstance 
        WHERE  AssignedTo = @AssignedTo 
          AND  Status IN (1, 2, 3) 
          AND  IsDeleted = 0 
        ORDER BY SLADeadline ASC"
    $cmd.Parameters.AddWithValue("@AssignedTo", $username) | Out-Null
    
    $stepInstances = New-Object System.Collections.Generic.List[PSObject]
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) {
        $stepInstances.Add([PSCustomObject]@{
            WFStepInstanceId = $reader['WFStepInstanceId']
            WFInstanceId = $reader['WFInstanceId']
            WFStepId = $reader['WFStepId']
            AssignedTo = $reader['AssignedTo']
            Status = $reader['Status']
            IsDeleted = $reader['IsDeleted']
        })
    }
    $reader.Close()
    
    Write-Host "Found $($stepInstances.Count) step instances."
    
    # 2. Replicate loop
    foreach ($si in $stepInstances) {
        Write-Host "Processing StepInstanceId: $($si.WFStepInstanceId), InstanceId: $($si.WFInstanceId)"
        
        # Get workflow instance
        $cmdInst = $conn.CreateCommand()
        $cmdInst.CommandText = "
            SELECT WFInstanceId as WorkflowInstanceId, WFDefinitionId as WorkflowDefinitionId, TemplateVersion, 
                   InitiatedBy, InitiatedDate, CurrentStepId, Status, 
                   CompletedDate, InputParameters, IsDeleted, DeletedDate 
            FROM   WFE.t_WFInstance 
            WHERE  WFInstanceId = @WorkflowInstanceId 
              AND  IsDeleted = 0"
        $cmdInst.Parameters.AddWithValue("@WorkflowInstanceId", $si.WFInstanceId) | Out-Null
        
        $instReader = $cmdInst.ExecuteReader()
        $hasInstance = $instReader.Read()
        if ($hasInstance) {
            $wfDefinitionId = $instReader['WorkflowDefinitionId']
            $status = $instReader['Status']
            Write-Host "  -> Instance exists. WFDefinitionId: $wfDefinitionId, Status: $status"
        } else {
            Write-Host "  -> Instance NOT found or IsDeleted = 1"
        }
        $instReader.Close()
        
        if ($hasInstance) {
            # Get template
            $cmdTempl = $conn.CreateCommand()
            $cmdTempl.CommandText = "
                SELECT TOP 1 
                    WFDefinitionId as WorkflowDefinitionId, WFName as Name, IsDeleted
                FROM WFE.s_WFDefinition 
                WHERE WFDefinitionId = @WorkflowDefinitionId 
                  AND IsDeleted = 0"
            $cmdTempl.Parameters.AddWithValue("@WorkflowDefinitionId", $wfDefinitionId) | Out-Null
            $templReader = $cmdTempl.ExecuteReader()
            if ($templReader.Read()) {
                Write-Host "  -> Template exists. Name: $($templReader['Name'])"
            } else {
                Write-Host "  -> Template NOT found or IsDeleted = 1"
            }
            $templReader.Close()
        }
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)"
} finally {
    if ($conn.State -eq 'Open') { $conn.Close() }
}
