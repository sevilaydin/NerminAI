FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY NerminAI.Domain/NerminAI.Domain.csproj NerminAI.Domain/
COPY NerminAI.Application/NerminAI.Application.csproj NerminAI.Application/
COPY NerminAI.Infrastructure/NerminAI.Infrastructure.csproj NerminAI.Infrastructure/
COPY NerminAI.API/NerminAI.API.csproj NerminAI.API/

# Restore
RUN dotnet restore NerminAI.API/NerminAI.API.csproj

# Copy everything and build
COPY . .
RUN dotnet publish NerminAI.API/NerminAI.API.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NerminAI.API.dll"]
