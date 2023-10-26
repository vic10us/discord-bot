FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["bot/bot.csproj", "bot/"]
COPY ["v10.Bot.Discord/v10.Bot.Discord.csproj", "v10.Bot.Discord/"]
COPY ["v10.Data.MongoDB/v10.Data.MongoDB.csproj", "v10.Data.MongoDB/"]
COPY ["v10.Bot.Core/v10.Bot.Core.csproj", "v10.Bot.Core/"]
COPY ["v10.Data.Abstractions/v10.Data.Abstractions.csproj", "v10.Data.Abstractions/"]
COPY ["v10.Events.Core/v10.Events.Core.csproj", "v10.Events.Core/"]
COPY ["v10.Messaging/v10.Messaging.csproj", "v10.Messaging/"]
COPY ["v10.Games.Dice/v10.Games.Dice.csproj", "v10.Games.Dice/"]
COPY ["v10.Games.EightBall/v10.Games.EightBall.csproj", "v10.Games.EightBall/"]
COPY ["v10.Bot.Core.Abstractions/v10.Bot.Core.Abstractions.csproj", "v10.Bot.Core.Abstractions/"]
COPY ["v10.Services.DadJokes/v10.Services.DadJokes.csproj", "v10.Services.DadJokes/"]
COPY ["v10.Services.Jokes/v10.Services.Jokes.csproj", "v10.Services.Jokes/"]
COPY ["v10.DependencyInjection.Microsoft/v10.DependencyInjection.Microsoft.csproj", "v10.DependencyInjection.Microsoft/"]
COPY ["v10.DependencyInjection.Abstractions/v10.DependencyInjection.Abstractions.csproj", "v10.DependencyInjection.Abstractions/"]
COPY ["v10.Options.Microsoft/v10.Options.Microsoft.csproj", "v10.Options.Microsoft/"]
COPY ["v10.Options.Abstractions/v10.Options.Abstractions.csproj", "v10.Options.Abstractions/"]
COPY ["v10.Services.Images/v10.Services.Images.csproj", "v10.Services.Images/"]
COPY ["v10.Services.MondayQuotes/v10.Services.MondayQuotes.csproj", "v10.Services.MondayQuotes/"]
COPY ["v10.Services.RedneckJokes/v10.Services.RedneckJokes.csproj", "v10.Services.RedneckJokes/"]
COPY ["v10.Services.StrangeLaws/v10.Services.StrangeLaws.csproj", "v10.Services.StrangeLaws/"]
RUN dotnet restore "bot/bot.csproj"
COPY . .
WORKDIR "/src/bot"
RUN dotnet build "bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "bot.dll"]