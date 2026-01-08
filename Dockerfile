FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file
COPY ["Event booking.Api.csproj", "."]
RUN dotnet restore "./Event booking.Api.csproj"

COPY . .
RUN dotnet build "Event booking.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Event booking.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Event booking.Api.dll"]  # Note: SPACE not hyphen