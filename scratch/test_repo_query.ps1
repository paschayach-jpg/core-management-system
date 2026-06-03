$connStr = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

# Test query with pitakpong
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT WFStepInstanceId, WFInstanceId, WFStepId, AssignedTo, AssignedDate, Status, CompletedDate, CompletedBy, OutputData, SLADeadline, SLAStatus, IsDeleted FROM WFE.t_WFStepInstance WHERE AssignedTo = @AssignedTo AND Status IN (1, 2, 3) AND IsDeleted = 0"
$cmd.Parameters.AddWithValue("@AssignedTo", "pitakpong")
$reader = $cmd.ExecuteReader()
Write-Output "--- Query Results for pitakpong ---"
$count = 0
while ($reader.Read()) {
    $count++
    $row = @{}
    for ($i = 0; $i -lt $reader.FieldCount; $i++) {
        $name = $reader.GetName($i)
        $val = $reader.GetValue($i)
        $row[$name] = $val
    }
    Write-Output ($row | ConvertTo-Json -Compress)
}
$reader.Close()
Write-Output "Total returned: $count"

$conn.Close()
