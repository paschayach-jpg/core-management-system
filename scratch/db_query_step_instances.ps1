$connStr = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

# Query all columns from WFE.t_WFStepInstance
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT * FROM WFE.t_WFStepInstance"
$reader = $cmd.ExecuteReader()
Write-Output "--- Content of WFE.t_WFStepInstance ---"
while ($reader.Read()) {
    $row = @{}
    for ($i = 0; $i -lt $reader.FieldCount; $i++) {
        $name = $reader.GetName($i)
        $val = $reader.GetValue($i)
        $row[$name] = $val
    }
    Write-Output ($row | ConvertTo-Json -Compress)
}
$reader.Close()

$conn.Close()
