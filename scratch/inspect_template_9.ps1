$connectionString = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT TemplateDefinition FROM WFE.s_WFDefinition WHERE WFDefinitionId = 9"
    $val = $cmd.ExecuteScalar()
    if ($val -ne [DBNull]::Value -and $val -ne $null) {
        Write-Output $val
    } else {
        Write-Output "No TemplateDefinition found or it is null."
    }
} catch {
    Write-Error $_.Exception.Message
} finally {
    $conn.Close()
}
