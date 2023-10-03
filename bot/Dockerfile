#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["bot/bot.csproj", "bot/"]
COPY ["v10.Services.DadJokes/v10.Services.DadJokes.csproj", "v10.Services.DadJokes/"]
COPY ["v10.Services.Jokes/v10.Services.Jokes.csproj", "v10.Services.Jokes/"]
COPY ["v10.Options.Microsoft/v10.Options.Microsoft.csproj", "v10.Options.Microsoft/"]
COPY ["v10.Options.Abstractions/v10.Options.Abstractions.csproj", "v10.Options.Abstractions/"]
COPY ["v10.DependencyInjection.Microsoft/v10.DependencyInjection.Microsoft.csproj", "v10.DependencyInjection.Microsoft/"]
COPY ["v10.DependencyInjection.Abstractions/v10.DependencyInjection.Abstractions.csproj", "v10.DependencyInjection.Abstractions/"]
COPY ["v10.Data.Abstractions/v10.Data.Abstractions.csproj", "v10.Data.Abstractions/"]
COPY ["v10.Data.MongoDB/v10.Data.MongoDB.csproj", "v10.Data.MongoDB/"]
COPY ["v10.Bot.Core/v10.Bot.Core.csproj", "v10.Bot.Core/"]
RUN dotnet restore "bot/bot.csproj"
COPY . .
WORKDIR "/src/bot"
RUN dotnet build "bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["bot/stanford-corenlp-models/**", "/app/stanford-corenlp-models"]
ENTRYPOINT ["dotnet", "bot.dll"]