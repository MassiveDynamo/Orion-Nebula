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

$systems = Get-ChildItem $JournalPath -Recurse -Filter "*.log" |
    ForEach-Object {
        Get-Content $_.FullName | ForEach-Object {
            try {
                    $edEvent = $_ | ConvertFrom-Json
                    if ($edEvent.event -eq "FSSAllBodiesFound") {
                        # Only process SystemNames that are in the systems-search CSV files
                        if ($edEvent.SystemName -notin $SearchSystemNames) {
                            Write-Host "Skipping '$($edEvent.SystemName)' because it is not in the systems-search CSV files."
                            return
                        }

                    $timestamp = [DateTime]::Parse(
                        $edEvent.timestamp,
                        [Globalization.CultureInfo]::InvariantCulture,
                        [Globalization.DateTimeStyles]::AssumeUniversal
                    ).ToUniversalTime()

                    if ($timestamp -ge $Since) {
                        [PSCustomObject]@{
                            SystemName = $edEvent.SystemName
                            SystemAddress = $edEvent.SystemAddress
                            CompletedUtc = $timestamp
                        }
                    }
                }
            } catch {
                # Ignore non-JSON or malformed journal lines
            }
        }
    } |
    Sort-Object SystemAddress, CompletedUtc -Descending |
    Group-Object SystemAddress |
    ForEach-Object { $_.Group[0] } |
    Where-Object { $_.SystemName -in $SearchSystemNames }

$systems | Export-Csv "d:\temp\fully-scanned-last-$Days-days.csv" `
    -NoTypeInformation -Encoding UTF8

$systems.Count