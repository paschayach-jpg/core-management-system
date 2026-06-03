$connStr = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

# Query s_WFStep
$cmd1 = $conn.CreateCommand()
$cmd1.CommandText = "SELECT * FROM WFE.s_WFStep"
$reader1 = $cmd1.ExecuteReader()
Write-Output "--- Content of WFE.s_WFStep ---"
while ($reader1.Read()) {
    $row = @{}
    for ($i = 0; $i -lt $reader1.FieldCount; $i++) {
        $name = $reader1.GetName($i)
        $val = $reader1.GetValue($i)
        $row[$name] = $val
    }
    Write-Output ($row | ConvertTo-Json -Compress)
}
$reader1.Close()

# Query s_WFDefinition
$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = "SELECT * FROM WFE.s_WFDefinition"
$reader2 = $cmd2.ExecuteReader()
Write-Output "--- Content of WFE.s_WFDefinition ---"
while ($reader2.Read()) {
    $row = @{}
    for ($i = 0; $i -lt $reader2.FieldCount; $i++) {
        $name = $reader2.GetName($i)
        $val = $reader2.GetValue($i)
        $row[$name] = $val
    }
    Write-Output ($row | ConvertTo-Json -Compress)
}
$reader2.Close()

$conn.Close()
