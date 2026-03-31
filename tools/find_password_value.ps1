# Dig deeper into specific password patterns
$targets = @(
    'D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.Core.dll',
    'D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.ProjectcenterCore.dll',
    'D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.DotNetZipWrapper.dll'
)

foreach ($path in $targets) {
    if (-not (Test-Path $path)) { continue }
    $name = [System.IO.Path]::GetFileName($path)
    Write-Host "`n=== $name ==="

    $bytes = [System.IO.File]::ReadAllBytes($path)
    $text = [System.Text.Encoding]::UTF8.GetString($bytes)

    # Look for fCorrectPassword and surrounding context
    $idx = 0
    while (($idx = $text.IndexOf('fCorrectPassword', $idx)) -ge 0) {
        $start = [Math]::Max(0, $idx - 80)
        $len = [Math]::Min(300, $text.Length - $start)
        $context = ($text.Substring($start, $len) -replace '[^\x20-\x7E]','.')
        Write-Host "`n  fCorrectPassword at $idx =>`n  $context"
        $idx += 16
    }

    # Look for FPasswordType - might reveal password mechanism
    $idx = 0
    while (($idx = $text.IndexOf('FPasswordType', $idx)) -ge 0) {
        $start = [Math]::Max(0, $idx - 80)
        $len = [Math]::Min(300, $text.Length - $start)
        $context = ($text.Substring($start, $len) -replace '[^\x20-\x7E]','.')
        Write-Host "`n  FPasswordType at $idx =>`n  $context"
        $idx += 13
    }

    # Look for DongleSecurityNeedPassword and nearby
    $idx = 0
    while (($idx = $text.IndexOf('DongleSecurityNeedPassword', $idx)) -ge 0) {
        $start = [Math]::Max(0, $idx - 40)
        $len = [Math]::Min(300, $text.Length - $start)
        $context = ($text.Substring($start, $len) -replace '[^\x20-\x7E]','.')
        Write-Host "`n  DongleSecurityNeedPassword at $idx =>`n  $context"
        $idx += 26
    }

    # Search for literal string constants near ZIP operations
    # Look for obfuscated names that might be password setters
    foreach ($p in @('_MLEr0C2Ntay','_iysGf7a0nLAV','_vdSh2TUsZQgd','_s2fLyUQDq10','_rwHlZFI4rxI','_ybaTrsYLvEOd','_K5')) {
        $idx = $text.IndexOf($p)
        if ($idx -ge 0) {
            $start = [Math]::Max(0, $idx - 20)
            $len = [Math]::Min(200, $text.Length - $start)
            $context = ($text.Substring($start, $len) -replace '[^\x20-\x7E]','.')
            Write-Host "`n  Obfuscated '$p' at $idx =>`n  $context"
        }
    }

    # Search for OZP-specific handling - look for .ozp or .OZP string
    foreach ($p in @('.ozp','.OZP','PREVIEWS','previews')) {
        $idx = 0
        while (($idx = $text.IndexOf($p, $idx)) -ge 0) {
            $start = [Math]::Max(0, $idx - 60)
            $len = [Math]::Min(200, $text.Length - $start)
            $context = ($text.Substring($start, $len) -replace '[^\x20-\x7E]','.')
            Write-Host "`n  '$p' at $idx =>`n  $context"
            $idx += $p.Length
        }
    }
}
