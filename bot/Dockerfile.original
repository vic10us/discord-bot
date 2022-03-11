FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
#COPY ["bot/bot.csproj", "bot/"]
COPY . .
RUN dotnet restore "bot/bot.csproj"
WORKDIR "/src/bot"
RUN dotnet build "bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DISCORD__TOKEN=aaa
ENTRYPOINT ["dotnet", "bot.dll"]
