$dlls = Get-ChildItem 'D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.*.dll'
foreach ($dll in $dlls) {
    try {
        $bytes = [System.IO.File]::ReadAllBytes($dll.FullName)
        $text = [System.Text.Encoding]::UTF8.GetString($bytes)
        $patterns = @('password','Password','PASSWORD','PassWord','Passwor','zipPass','ZipPassword','OrgaZip','orgaZip','OzpPass','ozpPass','archivePass','ArchivePass')
        foreach ($p in $patterns) {
            $idx = $text.IndexOf($p)
            if ($idx -ge 0) {
                $start = [Math]::Max(0, $idx - 30)
                $len = [Math]::Min(100, $text.Length - $start)
                $context = ($text.Substring($start, $len) -replace '[^\x20-\x7E]','.')
                Write-Host "$($dll.Name): '$p' at $idx => $context"
            }
        }
    } catch {
        # skip load errors
    }
}
