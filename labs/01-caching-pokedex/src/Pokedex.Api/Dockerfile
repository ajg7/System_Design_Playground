FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Pokedex.Api/Pokedex.Api.csproj src/Pokedex.Api/
RUN dotnet restore src/Pokedex.Api/Pokedex.Api.csproj
COPY src/Pokedex.Api/ src/Pokedex.Api/
RUN dotnet publish src/Pokedex.Api/Pokedex.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Pokedex.Api.dll"]
