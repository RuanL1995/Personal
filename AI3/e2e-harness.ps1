$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$env:VACATION_WEBHOOK_SECRET = if ($env:VACATION_WEBHOOK_SECRET) { $env:VACATION_WEBHOOK_SECRET } else { 'synthetic-test-secret' }
$process = Start-Process dotnet -ArgumentList "run --project `"$root\AI2\apiTest`" --no-launch-profile" -PassThru -WindowStyle Hidden
try {
  $base = 'http://localhost:5080'; for ($i=0; $i -lt 30; $i++) { try { if ((Invoke-RestMethod "$base/health").status -eq 'ok') { break } } catch { Start-Sleep -Milliseconds 300 } }
  $headers = @{ 'Idempotency-Key' = 'e2e-search'; 'X-Correlation-ID' = 'e2e-correlation' }
  $search = Invoke-RestMethod "$base/api/flights/search" -Method Post -Headers $headers -ContentType 'application/json' -Body '{"origin":"JFK","destination":"LHR","departureDate":"2030-06-01","travelers":1}'
  $repeatSearch = Invoke-RestMethod "$base/api/flights/search" -Method Post -Headers $headers -ContentType 'application/json' -Body '{"origin":"JFK","destination":"LHR","departureDate":"2030-06-01","travelers":1}'
  if ($search.flights[0].flightId -ne $repeatSearch.flights[0].flightId) { throw 'Search idempotency check failed.' }
  $flight = $search.flights[0].flightId
  $bookingHeaders = @{ 'Idempotency-Key' = 'e2e-booking' }
  $bookingBody = @{ flightId = $flight; travelerName = 'Synthetic Traveler' } | ConvertTo-Json
  $booking = Invoke-RestMethod "$base/api/bookings" -Method Post -Headers $bookingHeaders -ContentType 'application/json' -Body $bookingBody
  $repeatBooking = Invoke-RestMethod "$base/api/bookings" -Method Post -Headers $bookingHeaders -ContentType 'application/json' -Body $bookingBody
  if ($booking.bookingId -ne $repeatBooking.bookingId) { throw 'Booking idempotency check failed.' }
  $body = ('{"eventId":"' + [guid]::NewGuid().ToString() + '","bookingId":"' + $booking.bookingId + '","status":"confirmed"}')
  $secret = $env:VACATION_WEBHOOK_SECRET
  $hmac = [System.Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($secret)); $sig = ([BitConverter]::ToString($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($body))).Replace('-', '')).ToLowerInvariant()
  try { Invoke-RestMethod "$base/api/webhooks/booking" -Method Post -Headers @{ 'X-Webhook-Signature' = 'invalid' } -ContentType 'application/json' -Body $body | Out-Null; throw 'Invalid webhook signature was accepted.' } catch { if ($_.Exception.Response.StatusCode.value__ -ne 401) { throw } }
  Invoke-RestMethod "$base/api/webhooks/booking" -Method Post -Headers @{ 'X-Webhook-Signature' = $sig } -ContentType 'application/json' -Body $body | Out-Null
  Invoke-RestMethod "$base/api/pad/callback" -Method Post -ContentType 'application/json' -Body (@{ runId = [guid]::NewGuid(); status = 'completed' } | ConvertTo-Json) | Out-Null
  Write-Output 'E2E harness passed: search, booking, signed webhook, PAD callback.'
} finally { Stop-Process -Id $process.Id -Force }

