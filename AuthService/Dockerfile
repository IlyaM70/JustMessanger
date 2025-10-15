# 1️⃣ Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy the rest of the code
COPY . .

# Build and publish the app into /app/publish
RUN dotnet publish -c Release -o /app/publish

# 2️⃣ Runtime stage (smaller image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Tell ASP.NET Core to listen on port 5027
ENV ASPNETCORE_URLS=http://+:5027

# Expose port 5027 for container traffic
EXPOSE 5027

# Start the app
ENTRYPOINT ["dotnet", "AuthService.dll"]
