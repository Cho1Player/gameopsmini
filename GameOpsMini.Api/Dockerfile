FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY GameOpsMini.Shared/GameOpsMini.Shared.csproj \
     GameOpsMini.Shared/

COPY GameOpsMini.Api/GameOpsMini.Api.csproj \
     GameOpsMini.Api/

RUN dotnet restore \
    GameOpsMini.Api/GameOpsMini.Api.csproj

COPY GameOpsMini.Shared/ GameOpsMini.Shared/
COPY GameOpsMini.Api/ GameOpsMini.Api/

RUN dotnet publish \
    GameOpsMini.Api/GameOpsMini.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "GameOpsMini.Api.dll"]