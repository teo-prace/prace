# Etapa 1: compila e publica
FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
COPY src/Portfolio.Web/Portfolio.Web.csproj src/Portfolio.Web/
RUN dotnet restore src/Portfolio.Web/Portfolio.Web.csproj
COPY src/ src/
RUN dotnet publish src/Portfolio.Web/Portfolio.Web.csproj -c Release -o /app --no-restore

# Etapa 2: imagem final, só com o runtime
FROM mcr.microsoft.com/dotnet/aspnet:11.0
WORKDIR /app
# A nuvem termina o HTTPS antes do contêiner; isto faz o app enxergar o esquema original.
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Portfolio.Web.dll"]
