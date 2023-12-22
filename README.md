# Docker

```cmd
docker run -it --rm -v %AppData%\Microsoft\UserSecrets\100ad8ba-7a2d-4fae-b60d-72f13a1e87e5\secrets.json:/app/hostsettings.json ghcr.io/vic10us/discord-bot
or
docker run -it --rm -v %AppData%\Microsoft\UserSecrets:/root/.microsoft/usersecrets ghcr.io/vic10us/discord-bot
```
