$connectionString = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$sqlFile = "c:\Users\sa463\OneDrive\Documents\cms_import\CMS.Data\Scripts\099_Fix_WFStepInstance_Columns.sql"

$content = [System.IO.File]::ReadAllText($sqlFile)
# Split script by "GO" lines
$batches = [System.Text.RegularExpressions.Regex]::Split($content, "(?mi)^\s*GO\s*$")

$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $conn.Open()
    Write-Output "Executing SQL batches on CMS_DB..."
    foreach ($batch in $batches) {
        $trimmed = $batch.Trim()
        if (([string]::IsNullOrWhiteSpace($trimmed))) { continue }
        
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $trimmed
        try {
            $cmd.ExecuteNonQuery() > $null
            Write-Output "Batch executed successfully."
        } catch {
            Write-Error "Error executing batch: $_"
            Write-Error "Query was: $trimmed"
            break
        }
    }
    Write-Output "SQL execution completed successfully."
} catch {
    Write-Error "Database connection error: $_"
} finally {
    $conn.Close()
}
