$response = Invoke-RestMethod "$env:JENKINS_QUEUE_ITEM/api/json"

if ($response.StatusCode -ge 400) {
    "Could not access jenkins queue item"
    exit(1)
}

$buildUrl = $response.executable.url;

$status = $null

while ($status -eq $null) {
    $response = Invoke-RestMethod "$buildUrl/api/json"
    if ($response.StatusCode -ge 400) {
        "Could not access jenkins build item"
        exit(1)
    }

    $status = $response.result;

    Start-Sleep -s 20
}

if ($status -eq "SUCCESS") {
    net use J: \\$env:FILES_ACCOUNT_NAME.file.core.windows.net\jenkins /u:AZURE\$env:FILES_ACCOUNT_NAME $env:FILES_ACCOUNT_KEY

    Get-ChildItem "J:/builds/$env:APPVEYOR_BUILD_VERSION/Packages" | % {
        Push-AppveyorArtifact $_.FullName
    }

    Get-ChildItem "J:/builds/$env:APPVEYOR_BUILD_VERSION/SiteExtensions" | % {
        Push-AppveyorArtifact $_.FullName
    }
} else {
    "Jenkins signed build failed"
}