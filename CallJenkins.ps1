$url = "https://$env:JENKINS_URL/job/Functions_webjobs_sdk/buildWithParameters?token=$env:JENKINS_TOKEN&APPVEYOR_BUILD_VERSION=$APPVEYOR_BUILD_VERSION&COMMIT_ID=$APPVEYOR_REPO_COMMIT"

$response = Invoke-WebRequest $url -Method Post
Set-AppveyorBuildVariable -Name "JENKINS_QUEUE_ITEM" -Value $response.Headers["Location"]
