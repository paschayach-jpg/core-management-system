$connString = 'Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;'
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
try {
    $conn.Open()
    
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT AuditID, AuditDate, UserName, Action, Result FROM audit.AuditLog WHERE Action = 'LOGIN' AND AuditDate >= '2026-06-03 00:00:00' ORDER BY AuditID DESC"
    $reader = $cmd.ExecuteReader()
    Write-Host '--- Logins today ---'
    while ($reader.Read()) {
        Write-Host ('{0} | {1} | {2} | {3}' -f $reader['AuditID'], $reader['AuditDate'], $reader['UserName'], $reader['Result'])
    }
    $reader.Close()

} catch {
    Write-Host "Error: $($_.Exception.Message)"
} finally {
    if ($conn.State -eq 'Open') { $conn.Close() }
}
