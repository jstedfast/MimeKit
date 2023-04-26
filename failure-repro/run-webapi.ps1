$ComposeFiles = @(
  "-f"
  "docker-compose.webapi.yaml"
)

docker-compose $ComposeFiles build --pull

if ($? -eq $false) {
  exit
}

docker-compose $ComposeFiles up --force-recreate --renew-anon-volumes --remove-orphans
