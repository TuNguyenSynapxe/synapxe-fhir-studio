FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/services/schema-parser-service/src/SchemaParserService.csproj", "schema-parser-service/"]
RUN dotnet restore "schema-parser-service/SchemaParserService.csproj"
COPY backend/services/schema-parser-service/src/ schema-parser-service/
WORKDIR "/src/schema-parser-service"
RUN dotnet build "SchemaParserService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SchemaParserService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchemaParserService.dll"]
