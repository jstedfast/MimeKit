$ComposeFiles = @(
  "-f"
  "docker-compose.webapi.yaml"
  "-f"
  "docker-compose.webapi.integration-tests.yaml"
)

docker-compose $ComposeFiles build --pull

if ($? -eq $false) {
  exit
}

docker-compose $ComposeFiles up --force-recreate --renew-anon-volumes --remove-orphans --abort-on-container-exit
