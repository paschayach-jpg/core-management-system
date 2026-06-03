$connString = 'Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;'
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
try {
    $conn.Open()
    
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT WFInstanceId, InitiatedDate, InitiatedBy, Status FROM WFE.t_WFInstance"
    $reader = $cmd.ExecuteReader()
    Write-Host '--- All WFInstance records ---'
    while ($reader.Read()) {
        Write-Host ('ID: {0} | Date: {1} | By: {2} | Status: {3}' -f $reader['WFInstanceId'], $reader['InitiatedDate'], $reader['InitiatedBy'], $reader['Status'])
    }
    $reader.Close()

} catch {
    Write-Host "Error: $($_.Exception.Message)"
} finally {
    if ($conn.State -eq 'Open') { $conn.Close() }
}
