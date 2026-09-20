$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'websiteTest'
$process = Start-Process dotnet -ArgumentList "run --project `"$project`" --no-launch-profile" -PassThru -WindowStyle Hidden
try {
  $base = 'http://localhost:5190'
  for ($i = 0; $i -lt 30; $i++) {
    try {
      if ((Invoke-RestMethod "$base/health").status -eq 'ok') { break }
    } catch {
      Start-Sleep -Milliseconds 300
    }
  }
  $health = Invoke-RestMethod "$base/health"
  if (-not $health.browserHosted -or -not $health.synthetic) { throw 'AI3 website health check failed.' }
  $page = Invoke-WebRequest "$base/" -UseBasicParsing
  if ($page.Content -notmatch 'AI3 Synthetic Vacation Planner') { throw 'AI3 website content was not served.' }
  Write-Output 'AI3 website smoke check passed.'
} finally {
  Stop-Process -Id $process.Id -Force
}
