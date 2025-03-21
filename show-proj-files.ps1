# Change to the script's own directory reliably
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -Path $scriptDir

# Files explicitly required
$requiredFiles = @(
    "Program.cs",
    "*.csproj",
    "Pages/*.cshtml",               # Fixed relative paths without `./`
    "Pages/*.cshtml.cs",            # Fixed relative paths without `./`
    "Models/*.cs",
    "Data/*.cs",
    "wwwroot/index.html",
    "Areas/Identity/Pages/Account/*.cshtml",
    "Areas/Identity/Pages/Account/*.cshtml.cs",
    "Areas/Identity/Pages/Account/Manage/Enable*.cshtml",
    "Areas/Identity/Pages/Account/Manage/Enable*.cshtml.cs",
    "*.sh",
    "*.env",
    "appsettings.json",
    "appsettings.*.json",
    "Dockerfile.*",
    "tests/*.ts"
)

# Explicitly excluding unnecessary directories
$excludeDirs = @(
    "bin",
    "obj",
    ".git",
    "tests/node_modules",
    ".github",
    "Areas/Identity"
)

# Function to check if a directory is excluded
function IsExcluded($path) {
    foreach ($excludeDir in $excludeDirs) {
        if ($path -like "*$excludeDir*") {
            return $true
        }
    }
    return $false
}

# Display contents of required files
$output = "=== Required Files Content ===`r`n"
foreach ($file in $requiredFiles) {
    $files = Get-ChildItem -Path $scriptDir -Filter $file -Recurse -ErrorAction SilentlyContinue
    foreach ($f in $files) {
        # Skip excluded directories
        if (IsExcluded $f.DirectoryName) {
            continue
        }

        $output += "---- Start of: $($f.FullName) ----`r`n"
        $content = Get-Content $f.FullName
        if ($content) {
            $output += $content -join "`r`n"
        } else {
            $output += "No content in file $($f.FullName)`r`n"
        }
        $output += "`r`n---- End of: $($f.FullName) ----`r`n"
    }
}

# Verifying existence of key JS resources
$output += "=== Resource File checks ===`r`n"
$jsFiles = @(
    "wwwroot/js/qr.js",
    "wwwroot/lib/qrcode.js"
)

foreach ($jsfile in $jsFiles) {
    if (Test-Path -Path $jsfile) {
        $output += "$($jsfile): Exists`r`n"
    } else {
        $output += "$($jsfile): Missing`r`n"
    }
}

$output 

# End of script
