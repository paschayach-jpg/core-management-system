$connString = 'Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;'
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
try {
    $conn.Open()
    
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT WFStepInstanceId, WFInstanceId, AssignedTo, Status, CompletedDate FROM WFE.t_WFStepInstance"
    $reader = $cmd.ExecuteReader()
    Write-Host '--- t_WFStepInstance ---'
    while ($reader.Read()) {
        Write-Host ('StepInstId: {0} | WFInstId: {1} | AssignedTo: {2} | Status: {3} | CompletedDate: {4}' -f $reader['WFStepInstanceId'], $reader['WFInstanceId'], $reader['AssignedTo'], $reader['Status'], $reader['CompletedDate'])
    }
    $reader.Close()

} catch {
    Write-Host "Error: $($_.Exception.Message)"
} finally {
    if ($conn.State -eq 'Open') { $conn.Close() }
}
