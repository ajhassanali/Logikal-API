# Search ALL DLLs and EXEs in bin for password-related strings
$files = Get-ChildItem 'D:\LOGIKAL\LOGIKAL\bin\*.dll','D:\LOGIKAL\LOGIKAL\bin\*.exe' -ErrorAction SilentlyContinue
foreach ($f in $files) {
    try {
        $bytes = [System.IO.File]::ReadAllBytes($f.FullName)
        $text = [System.Text.Encoding]::UTF8.GetString($bytes)

        # Look for Password near Zip or Ozp or Archive context
        $patterns = @('Password','password','ZipCrypto','AES256','SetPassword','zipPassword','OzpPassword','archiveKey','EncryptionKey')
        foreach ($p in $patterns) {
            $idx = 0
            while (($idx = $text.IndexOf($p, $idx)) -ge 0) {
                $start = [Math]::Max(0, $idx - 40)
                $len = [Math]::Min(120, $text.Length - $start)
                $context = ($text.Substring($start, $len) -replace '[^\x20-\x7E]','.')
                Write-Host "$($f.Name): '$p' at $idx => $context"
                $idx += $p.Length
                break  # one match per pattern per file
            }
        }
    } catch {}
}
