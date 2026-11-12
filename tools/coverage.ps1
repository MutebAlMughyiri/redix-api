# How much of the API the tests run: every line and branch of src/Redix.Api, the migrations left out (tests/coverage.runsettings), as a report in
# coverage/report/index.html and a summary printed here. Uses coverlet, already in the test project, and ReportGenerator from the repo's tool manifest.
$root = Split-Path $PSScriptRoot
Set-Location $root
Remove-Item coverage -Recurse -Force -ErrorAction SilentlyContinue
dotnet tool restore | Out-Null
dotnet test --settings tests/coverage.runsettings --collect "XPlat Code Coverage" --results-directory coverage/raw 2>&1 | Select-Object -Last 2
dotnet tool run reportgenerator "-reports:coverage/raw/**/coverage.cobertura.xml" "-targetdir:coverage/report" "-reporttypes:Html;TextSummary" "-title:REDIX API" | Out-Null
Get-Content coverage/report/Summary.txt | Select-Object -First 16
