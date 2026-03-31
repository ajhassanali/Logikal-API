# Try to load the DLLs via .NET reflection and find the actual password value
# Focus on Ofcas.Lk.DotNetZipWrapper.dll and Ofcas.EPL.Framework.Windows.dll

Write-Host "=== Attempting .NET reflection to find ZIP password ==="

# Try DotNetZipWrapper first - it's a .NET assembly
try {
    $asm = [System.Reflection.Assembly]::LoadFrom('D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.DotNetZipWrapper.dll')
    Write-Host "`nLoaded: $($asm.FullName)"
    foreach ($type in $asm.GetTypes()) {
        Write-Host "`n  Type: $($type.FullName)"
        foreach ($method in $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::Instance)) {
            if ($method.Name -match 'Password|Zip|Extract|Archive|Encrypt') {
                Write-Host "    Method: $($method.Name) ($($method.GetParameters() | ForEach-Object { $_.ParameterType.Name + ' ' + $_.Name } | Join-String -Separator ', '))"
            }
        }
        foreach ($field in $type.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::Instance)) {
            if ($field.Name -match 'password|Password|key|Key|secret') {
                $val = $null
                if ($field.IsStatic) {
                    try { $val = $field.GetValue($null) } catch {}
                }
                Write-Host "    Field: $($field.Name) [$($field.FieldType.Name)] $(if ($val) { "= '$val'" } else { '' })"
            }
        }
        foreach ($prop in $type.GetProperties([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::Instance)) {
            if ($prop.Name -match 'password|Password|key|Key') {
                Write-Host "    Property: $($prop.Name) [$($prop.PropertyType.Name)]"
            }
        }
    }
} catch {
    Write-Host "Error loading DotNetZipWrapper: $_"
}

# Try EPL Framework Windows
try {
    $asm2 = [System.Reflection.Assembly]::LoadFrom('D:\LOGIKAL\LOGIKAL\bin\Ofcas.EPL.Framework.Windows.dll')
    Write-Host "`nLoaded: $($asm2.FullName)"
    foreach ($type in $asm2.GetTypes()) {
        foreach ($method in $type.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::Instance)) {
            if ($method.Name -match 'Password|Zip|Extract|Archive|Ozp') {
                Write-Host "  $($type.FullName).$($method.Name)"
            }
        }
        foreach ($field in $type.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::Instance)) {
            if ($field.Name -match 'password|Password|key|Key|secret') {
                $val = $null
                if ($field.IsStatic) {
                    try { $val = $field.GetValue($null) } catch {}
                }
                Write-Host "  $($type.FullName).$($field.Name) [$($field.FieldType.Name)] $(if ($val) { "= '$val'" } else { '' })"
            }
        }
    }
} catch {
    Write-Host "Error loading EPL Framework: $_"
}

# Also try ShadowFileService - has obfuscated password setter
try {
    $asm3 = [System.Reflection.Assembly]::LoadFrom('D:\LOGIKAL\LOGIKAL\bin\Ofcas.Lk.ShadowFileService.dll')
    Write-Host "`nLoaded: $($asm3.FullName)"
    foreach ($type in $asm3.GetTypes()) {
        foreach ($field in $type.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::Instance)) {
            if ($field.Name -match 'password|Password|key|Key|secret|_MLEr|_iysG|_vdSh|_s2fL') {
                $val = $null
                if ($field.IsStatic) {
                    try { $val = $field.GetValue($null) } catch {}
                }
                Write-Host "  $($type.FullName).$($field.Name) [$($field.FieldType.Name)] $(if ($val) { "= '$val'" } else { '' })"
            }
        }
    }
} catch {
    Write-Host "Error loading ShadowFileService: $_"
}
