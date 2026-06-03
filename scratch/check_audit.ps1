$connString = 'Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;'
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
try {
    $conn.Open()
    
    # Query recent audit log entries from SYSTEM
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT AuditID, AuditDate, UserName, Module, Action, TableName, RecordID, OldValue, NewValue FROM audit.AuditLog WHERE UserName = 'SYSTEM' AND AuditID >= 21014 ORDER BY AuditID DESC"
    $reader = $cmd.ExecuteReader()
    Write-Host '--- Recent SYSTEM Audit Logs ---'
    while ($reader.Read()) {
        Write-Host ('{0} | {1} | {2} | {3} | {4} | {5}' -f $reader['AuditID'], $reader['AuditDate'], $reader['TableName'], $reader['RecordID'], $reader['OldValue'], $reader['NewValue'])
    }
    $reader.Close()

} catch {
    Write-Host "Error: $($_.Exception.Message)"
} finally {
    if ($conn.State -eq 'Open') { $conn.Close() }
}
