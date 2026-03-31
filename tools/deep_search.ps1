# Deep search for ZIP password in key DLLs
# Focus on: Ofcas.Lk.DotNetZipWrapper.dll, Ofcas.Lk.ProjectcenterCore.dll, Ofcas.Lk.Core.dll

$targets = @(
    'D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.DotNetZipWrapper.dll',
    'D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.ProjectcenterCore.dll',
    'D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.Core.dll',
    'D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.ShadowFileService.dll',
    'D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.Project.MC.dll',
    'D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.Sso.Core.dll'
)

Write-Host "=== Searching for ZIP password in Logikal DLLs ==="

foreach ($path in $targets) {
    if (-not (Test-Path $path)) { continue }
    $name = [System.IO.Path]::GetFileName($path)
    Write-Host "`n--- $name ---"

    $bytes = [System.IO.File]::ReadAllBytes($path)
    $text = [System.Text.Encoding]::UTF8.GetString($bytes)

    # Search for SetPassword and nearby context
    $idx = 0
    while (($idx = $text.IndexOf('SetPassword', $idx)) -ge 0) {
        $start = [Math]::Max(0, $idx - 60)
        $len = [Math]::Min(200, $text.Length - $start)
        $context = ($text.Substring($start, $len) -replace '[^\x20-\x7E]','.')
        Write-Host "  SetPassword at $idx => $context"
        $idx += 11
    }

    # Search for FPassword field (Delphi naming convention for fields)
    $idx = 0
    while (($idx = $text.IndexOf('FPassword', $idx)) -ge 0) {
        $start = [Math]::Max(0, $idx - 40)
        $len = [Math]::Min(160, $text.Length - $start)
        $context = ($text.Substring($start, $len) -replace '[^\x20-\x7E]','.')
        Write-Host "  FPassword at $idx => $context"
        $idx += 9
    }

    # Search for TOrgaZip (the OZP handler class)
    $idx = 0
    while (($idx = $text.IndexOf('TOrgaZip', $idx)) -ge 0) {
        $start = [Math]::Max(0, $idx - 20)
        $len = [Math]::Min(300, $text.Length - $start)
        $context = ($text.Substring($start, $len) -replace '[^\x20-\x7E]','.')
        Write-Host "  TOrgaZip at $idx => $context"
        $idx += 8
    }

    # Search for hardcoded string literals that could be passwords
    # Look for sequences between quotes that are 4-20 chars
    $idx = 0
    while (($idx = $text.IndexOf('_password', $idx)) -ge 0) {
        $start = [Math]::Max(0, $idx - 60)
        $len = [Math]::Min(200, $text.Length - $start)
        $context = ($text.Substring($start, $len) -replace '[^\x20-\x7E]','.')
        Write-Host "  _password at $idx => $context"
        $idx += 9
    }
}
