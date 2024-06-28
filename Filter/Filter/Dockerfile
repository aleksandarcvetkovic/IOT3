
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Filter.csproj", "./"]
RUN dotnet restore "./Filter.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Filter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Filter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Filter.dll"]
