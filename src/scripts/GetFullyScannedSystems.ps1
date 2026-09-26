$Days = 30
$Since = (Get-Date).ToUniversalTime().AddDays(-$Days)
$JournalPath = "$env:USERPROFILE\Saved Games\Frontier Developments\Elite Dangerous"
$Path = Join-Path $PSScriptRoot "..\..\data\Spansh"
$SearchFiles = @(
    Get-ChildItem -Path @($PSScriptRoot, $Path) `
        -Filter "systems-search*.csv" -File -ErrorAction SilentlyContinue
) | Sort-Object FullName -Unique
$SearchSystemNames = @($SearchFiles | Import-Csv | Select-Object -ExpandProperty Name -Unique)

if (-not $SearchSystemNames) {
    throw "No systems-search CSV files were found in $PSScriptRoot or its parent folder."
}

$EventTypes = @{}

$systems = Get-ChildItem $JournalPath -Recurse -Filter "*.log" |
ForEach-Object {
    Get-Content $_.FullName | ForEach-Object {
        try {
            $edEvent = $_ | ConvertFrom-Json
            if (-not $EventTypes.ContainsKey([string]$edEvent.event)) {
                $EventTypes[[string]$edEvent.event] = $true
            }

            if( $edEvent.event -eq "FSDJump" ) {
                $systemAddressHex = '{0:X}' -f [long]$edEvent.SystemAddress
                Write-Host "$systemAddressHex '$($edEvent.StarSystem)'" ($edEvent.StarPos[0], $edEvent.StarPos[1], $edEvent.StarPos[2])
            }
        }
        catch {
            # Ignore non-JSON or malformed journal lines
        }
    }
}

$systems.Count

Write-Host "Event types found in journal files:"
$EventTypes.Keys | Sort-Object | ForEach-Object { Write-Host $_ }

