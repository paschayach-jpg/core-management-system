$connectionString = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "UPDATE WFE.t_WFInstance SET Status = 2, CompletedDate = NULL WHERE WFInstanceId = 28"
    $rows = $cmd.ExecuteNonQuery()
    Write-Output "Successfully updated t_WFInstance for ID 28. Rows affected: $rows"
} catch {
    Write-Error $_.Exception.Message
} finally {
    $conn.Close()
}
