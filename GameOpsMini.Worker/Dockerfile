FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY GameOpsMini.Shared/GameOpsMini.Shared.csproj \
    GameOpsMini.Shared/

COPY GameOpsMini.Worker/GameOpsMini.Worker.csproj \
    GameOpsMini.Worker/

RUN dotnet restore \
    GameOpsMini.Worker/GameOpsMini.Worker.csproj

COPY GameOpsMini.Shared/ GameOpsMini.Shared/
COPY GameOpsMini.Worker/ GameOpsMini.Worker/

RUN dotnet publish \
    GameOpsMini.Worker/GameOpsMini.Worker.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV DOTNET_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "GameOpsMini.Worker.dll"]