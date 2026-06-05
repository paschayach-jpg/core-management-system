$connectionString = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $conn.Open()
    Write-Output "=== WFE.t_WFInstance details ==="
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT WFInstanceId, WFDefinitionId, Status, InitiatedBy FROM WFE.t_WFInstance WHERE WFInstanceId = 34"
    $r = $cmd.ExecuteReader()
    $defId = 0
    if ($r.Read()) {
        $defId = $r["WFDefinitionId"]
        Write-Output ("InstanceId: " + $r["WFInstanceId"] + " | DefId: " + $defId + " | Status: " + $r["Status"] + " | InitiatedBy: " + $r["InitiatedBy"])
    }
    $r.Close()

    Write-Output "`n=== WFE.t_WFStepInstance details ==="
    $cmd.CommandText = "SELECT WFStepInstanceId, WFStepId, Status, AssignedTo, CompletedBy, CompletedDate FROM WFE.t_WFStepInstance WHERE WFInstanceId = 34 ORDER BY WFStepInstanceId"
    $r = $cmd.ExecuteReader()
    while ($r.Read()) {
        Write-Output ("StepInstanceId: " + $r["WFStepInstanceId"] + " | StepId: " + $r["WFStepId"] + " | Status: " + $r["Status"] + " | AssignedTo: " + $r["AssignedTo"] + " | CompletedBy: " + $r["CompletedBy"] + " | CompletedDate: " + $r["CompletedDate"])
    }
    $r.Close()

    if ($defId -gt 0) {
        Write-Output "`n=== WFE.s_WFStep details for DefId $defId ==="
        $cmd.CommandText = "SELECT WFStepId, WFDefinitionId, StepName, StepType, SequenceNumber FROM WFE.s_WFStep WHERE WFDefinitionId = $defId ORDER BY SequenceNumber"
        $r = $cmd.ExecuteReader()
        while ($r.Read()) {
            Write-Output ("StepId: " + $r["WFStepId"] + " | StepName: " + $r["StepName"] + " | StepType: " + $r["StepType"] + " | SequenceNumber: " + $r["SequenceNumber"])
        }
        $r.Close()
    }
} catch {
    Write-Error $_.Exception.Message
} finally {
    $conn.Close()
}
