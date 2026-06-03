$connectionString = "Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;"
$sqlFile = "c:\Users\sa463\OneDrive\Documents\cms_import\CMS.Data\Scripts\000_Diagnose_Schema.sql"

$content = [System.IO.File]::ReadAllText($sqlFile)
$batches = [System.Text.RegularExpressions.Regex]::Split($content, "(?mi)^\s*GO\s*$")

$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $conn.Open()
    
    # Register InfoMessage event handler to capture PRINT statements
    $printOutputs = New-Object System.Collections.Generic.List[string]
    $handler = [System.Data.SqlClient.SqlInfoMessageEventHandler] {
        param($sender, $eventArgs)
        foreach ($error in $eventArgs.Errors) {
            $printOutputs.Add($error.Message)
        }
    }
    $conn.add_InfoMessage($handler)
    $conn.FireInfoMessageEventOnUserErrors = $true

    foreach ($batch in $batches) {
        $trimmed = $batch.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed)) { continue }
        
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $trimmed
        
        # We want to support returning query result tables as well
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
        $ds = New-Object System.Data.DataSet
        try {
            $adapter.Fill($ds) > $null
            
            # Print captured PRINT statements so far
            foreach ($msg in $printOutputs) {
                Write-Output $msg
            }
            $printOutputs.Clear()

            # Format and display tables
            foreach ($table in $ds.Tables) {
                $table | Format-Table | Out-String | Write-Output
            }
        } catch {
            Write-Error "Error executing query: $_"
            Write-Error "Query was: $trimmed"
        }
    }
    
    # Print any remaining PRINT statements
    foreach ($msg in $printOutputs) {
        Write-Output $msg
    }
} catch {
    Write-Error "Connection failed: $_"
} finally {
    $conn.Close()
}
