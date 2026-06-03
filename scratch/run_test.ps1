$code = Get-Content -Raw -Path scratch/test_dock.cs
Add-Type -TypeDefinition $code -ReferencedAssemblies "System.Windows.Forms", "System.Drawing"
[System.Windows.Forms.Application]::Run((New-Object TestForm))
