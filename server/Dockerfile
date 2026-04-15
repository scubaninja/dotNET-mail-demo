# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Tailwind.Mail.csproj .
RUN dotnet restore Tailwind.Mail.csproj --configuration Release

COPY . .
RUN dotnet publish Tailwind.Mail.csproj --configuration Release --no-restore -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Tailwind.Mail.dll"]
