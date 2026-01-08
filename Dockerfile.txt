# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Create directory for uploaded files
RUN mkdir -p /app/wwwroot/uploads

# Expose port (Render assigns PORT environment variable)
EXPOSE 8080

# Set ASP.NET Core to listen on the port Render assigns
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Event-booking.Api.dll"]