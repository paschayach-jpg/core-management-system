$connectionString = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $conn.Open()
    Write-Output "--- Table Row Counts ---"
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT 'WFE.t_WFInstance' AS TableName, COUNT(*) AS Cnt FROM WFE.t_WFInstance UNION ALL SELECT 'WFE.t_WFStepInstance', COUNT(*) FROM WFE.t_WFStepInstance"
    $r = $cmd.ExecuteReader()
    while($r.Read()) {
        Write-Output ($r["TableName"] + ": " + $r["Cnt"])
    }
    $r.Close()
} catch {
    Write-Error $_.Exception.Message
} finally {
    $conn.Close()
}
