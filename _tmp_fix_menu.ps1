$p = 'c:\project\surakshaAR\Assets\AR_Fire_foundation\scripts\App\MenuUIController.cs'
$lines = Get-Content -LiteralPath $p
Write-Output ("BEFORE: " + $lines.Count)
$keep = $lines[0..469]   # keep lines 1..470 (clean through AnchorFill)
Set-Content -LiteralPath $p -Value $keep -Encoding UTF8
$check = Get-Content -LiteralPath $p
Write-Output ("AFTER: " + $check.Count)
Write-Output ($check[-3..-1] -join "`n")
