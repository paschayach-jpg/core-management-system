$connectionString = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $conn.Open()
    Write-Output "=== WFE.t_WFStepInstance details ==="
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT WFStepInstanceId, WFStepId, Status, OutputData, Remarks FROM WFE.t_WFStepInstance WHERE WFInstanceId = 28 ORDER BY WFStepInstanceId"
    $r = $cmd.ExecuteReader()
    while ($r.Read()) {
        Write-Output ("StepInstanceId: " + $r["WFStepInstanceId"] + " | StepId: " + $r["WFStepId"] + " | Status: " + $r["Status"] + " | OutputData: '" + $r["OutputData"] + "' | Remarks: '" + $r["Remarks"] + "'")
    }
    $r.Close()
} catch {
    Write-Error $_.Exception.Message
} finally {
    $conn.Close()
}
