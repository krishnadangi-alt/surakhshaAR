$p = 'c:\project\surakshaAR\Assets\AR_Fire_foundation\scripts\App\MenuUIController.cs'
$c = Get-Content -LiteralPath $p
Write-Output ("TOTAL_LINES=" + $c.Count)

# Sanity checks on boundaries
if ($c.Count -lt 1636) { Write-Output 'ERR: unexpected line count'; exit 2 }
if ($c[421] -notmatch 'private string L\(string key\)') { Write-Output 'ERR: L method not at expected line'; exit 2 }
if ($c[425] -ne '') { Write-Output 'ERR: line 426 not blank'; exit 2 }
if ($c[470] -notmatch '^\s*}\s*$') { Write-Output 'ERR: line 471 not a closing brace'; exit 2 }
if ($c[697] -ne '}') { Write-Output 'ERR: line 698 not class close brace'; exit 2 }

# Keep: lines 1-426 (index 0..425) + lines 471-698 (index 470..697)
$out = New-Object System.Collections.Generic.List[string]
for ($i = 0; $i -le 425; $i++) { $out.Add($c[$i]) }
for ($i = 470; $i -le 697; $i++) { $out.Add($c[$i]) }

Set-Content -LiteralPath $p -Value $out -Encoding UTF8
Write-Output ("NEW_TOTAL=" + (Get-Content -LiteralPath $p).Count)
Write-Output 'REPAIR_OK'
