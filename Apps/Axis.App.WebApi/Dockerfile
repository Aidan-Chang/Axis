#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Apps/Axis.App.WebApi/Axis.App.WebApi.csproj", "Apps/Axis.App.WebApi/"]
RUN dotnet restore "Apps/Axis.App.WebApi/Axis.App.WebApi.csproj"
COPY . .
WORKDIR "/src/Apps/Axis.App.WebApi"
RUN dotnet build "Axis.App.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Axis.App.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Axis.App.WebApi.dll"]