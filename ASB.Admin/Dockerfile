FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ASB.Admin/ASB.Admin.csproj ASB.Admin/
COPY ASB.Services/ASB.Services.csproj ASB.Services/
COPY ASB.Repositories/ASB.Repositories.csproj ASB.Repositories/
COPY ASB.ErrorHandler/ASB.ErrorHandler.csproj ASB.ErrorHandler/
COPY ASB.Authorization/ASB.Authorization.csproj ASB.Authorization/
COPY ASB.Agent/ASB.Agent.csproj ASB.Agent/
RUN dotnet restore ASB.Admin/ASB.Admin.csproj

# Copy everything and build
COPY . .
WORKDIR /src/ASB.Admin
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ASB.Admin.dll"]
