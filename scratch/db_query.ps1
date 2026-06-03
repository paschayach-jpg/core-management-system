$connStr = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

# List tables in WFE schema
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'WFE'"
$reader = $cmd.ExecuteReader()
Write-Output "--- Tables in WFE schema ---"
$tables = @()
while ($reader.Read()) {
    $tableName = $reader["TABLE_NAME"].ToString()
    $tables += $tableName
    Write-Output $tableName
}
$reader.Close()

# Show count of rows for each table
foreach ($table in $tables) {
    $cmdCount = $conn.CreateCommand()
    $cmdCount.CommandText = "SELECT COUNT(*) FROM WFE.$table"
    $count = $cmdCount.ExecuteScalar()
    Write-Output "Table WFE.$table count: $count"
}

$conn.Close()
