FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY GameOpsMini.DummyServer/GameOpsMini.DummyServer.csproj \
    GameOpsMini.DummyServer/

RUN dotnet restore \
    GameOpsMini.DummyServer/GameOpsMini.DummyServer.csproj

COPY GameOpsMini.DummyServer/ GameOpsMini.DummyServer/

RUN dotnet publish \
    GameOpsMini.DummyServer/GameOpsMini.DummyServer.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV PORT=7777

EXPOSE 7777

ENTRYPOINT ["dotnet", "GameOpsMini.DummyServer.dll"]