#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RAD.MassTransit.Helper/RAD.MassTransit.Helper.csproj", "RAD.MassTransit.Helper/"]
RUN dotnet restore "./RAD.MassTransit.Helper/RAD.MassTransit.Helper.csproj"
COPY . .
WORKDIR "/src/RAD.MassTransit.Helper"
RUN dotnet build "./RAD.MassTransit.Helper.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RAD.MassTransit.Helper.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RAD.MassTransit.Helper.dll"]