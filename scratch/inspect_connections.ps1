$connectionString = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $conn.Open()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT WFConnectionId, FromStepId, ToStepId, ConditionExpression FROM WFE.s_WFConnection WHERE WFDefinitionId = 9"
    $r = $cmd.ExecuteReader()
    while ($r.Read()) {
        Write-Output ("WFConnectionId: " + $r["WFConnectionId"] + " | FromStepId: " + $r["FromStepId"] + " | ToStepId: " + $r["ToStepId"] + " | Condition: " + $r["ConditionExpression"])
    }
    $r.Close()
} catch {
    Write-Error $_.Exception.Message
} finally {
    $conn.Close()
}
