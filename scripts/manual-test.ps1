$ErrorActionPreference = "Stop"

$base = "http://localhost:5151"

try {
  Write-Output "Step 0: Seed data"
  $seed = Invoke-RestMethod -Method Post -Uri "$base/api/dev/seed-data"
  $seed | ConvertTo-Json -Depth 10

  Write-Output "Step 1: Submit v1"
  $body1 = '{"PreviewFileAssetId":null,"SourceFileAssetId":null,"Notes":"Submit first manuscript version for review."}'
  $submit1 = Invoke-RestMethod -Method Post -Uri "$base/api/chapters/55555555-5555-5555-5555-555555555555/manuscripts" -Body $body1 -ContentType "application/json"
  $submit1 | ConvertTo-Json -Depth 10

  $manuscriptIdV1 = $submit1.data.id
  Write-Output "Manuscript V1 ID: $manuscriptIdV1"

  Write-Output "Step 2: Start review v1"
  $start1 = Invoke-RestMethod -Method Post -Uri "$base/api/manuscripts/$manuscriptIdV1/start-review"
  $start1 | ConvertTo-Json -Depth 10

  Write-Output "Step 3: Create annotation"
  $bodyAnno = '{"PageNo":1,"PositionX":45.50,"PositionY":62.10,"Content":"Face angle looks off here; please redraw for clarity."}'
  $anno = Invoke-RestMethod -Method Post -Uri "$base/api/manuscripts/$manuscriptIdV1/annotations" -Body $bodyAnno -ContentType "application/json"
  $anno | ConvertTo-Json -Depth 10

  Write-Output "Step 4: Request revision"
  $bodyRev = '{"Feedback":"Please fix the eye shape on page 1 as noted."}'
  $rev = Invoke-RestMethod -Method Post -Uri "$base/api/manuscripts/$manuscriptIdV1/request-revision" -Body $bodyRev -ContentType "application/json"
  $rev | ConvertTo-Json -Depth 10

  Write-Output "Step 5: Submit v2"
  $body2 = '{"PreviewFileAssetId":null,"SourceFileAssetId":null,"Notes":"Revised page 1 eye details. Please approve version 2."}'
  $submit2 = Invoke-RestMethod -Method Post -Uri "$base/api/chapters/55555555-5555-5555-5555-555555555555/manuscripts" -Body $body2 -ContentType "application/json"
  $submit2 | ConvertTo-Json -Depth 10

  $manuscriptIdV2 = $submit2.data.id
  Write-Output "Manuscript V2 ID: $manuscriptIdV2"

  Write-Output "Step 6: Start review v2"
  $start2 = Invoke-RestMethod -Method Post -Uri "$base/api/manuscripts/$manuscriptIdV2/start-review"
  $start2 | ConvertTo-Json -Depth 10

  Write-Output "Step 7: Approve v2"
  $approve = Invoke-RestMethod -Method Post -Uri "$base/api/manuscripts/$manuscriptIdV2/approve"
  $approve | ConvertTo-Json -Depth 10
}
catch {
  Write-Output "Error: $($_.Exception.Message)"
  if ($_.Exception.Response) {
    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
    $reader.ReadToEnd()
  }
}
