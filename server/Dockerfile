FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Tailwind.Mail.csproj .
RUN dotnet restore Tailwind.Mail.csproj --configuration Release

COPY . .
RUN dotnet publish Tailwind.Mail.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Tailwind.Mail.dll"]
