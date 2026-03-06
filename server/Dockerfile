# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY Tailwind.Mail.csproj ./
RUN dotnet restore Tailwind.Mail.csproj --configuration Release

# Copy source and publish
COPY . .
RUN dotnet publish Tailwind.Mail.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

COPY --from=build /app/publish .

# ASP.NET Core listens on port 8080 by default in containers
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

USER appuser
ENTRYPOINT ["dotnet", "Tailwind.Mail.dll"]
