FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY LowiskoWeb/LowiskoWeb.csproj LowiskoWeb/
RUN dotnet restore LowiskoWeb/LowiskoWeb.csproj
COPY LowiskoWeb/ LowiskoWeb/
WORKDIR /src/LowiskoWeb
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "LowiskoWeb.dll"]
