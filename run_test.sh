DOCKER_HOST="unix:///Users/beyondnet/.docker/run/docker.sock" dotnet test src/apps/ums.api/Ums.Presentation.IntegrationTest --filter "CreateUserAccount_ValidPayload_Returns201WithId" > test_out.txt
cat test_out.txt
