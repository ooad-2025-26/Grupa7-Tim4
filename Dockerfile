# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first to restore dependencies (leverages Docker cache)
COPY ZamETF/ZamETF/ZamETF.csproj ZamETF/ZamETF/
RUN dotnet restore ZamETF/ZamETF/ZamETF.csproj

# Copy the rest of the source files
COPY ZamETF/ZamETF/ ZamETF/ZamETF/

# Build and publish the application
WORKDIR /src/ZamETF/ZamETF
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 8080 (ASP.NET Core 8.0 default container port)
EXPOSE 8080

# Set environment variables for production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ZamETF.dll"]
