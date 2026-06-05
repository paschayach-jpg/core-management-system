$connectionString = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'WFE' AND TABLE_NAME = 's_WFStep'"
    $r = $cmd.ExecuteReader()
    while ($r.Read()) {
        Write-Output ($r["COLUMN_NAME"] + ": " + $r["DATA_TYPE"] + "(" + $r["CHARACTER_MAXIMUM_LENGTH"] + ")")
    }
    $r.Close()
} catch {
    Write-Error $_.Exception.Message
} finally {
    $conn.Close()
}
