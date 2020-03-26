#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Pensees.Charon.Web.Host/Pensees.Charon.Web.Host.csproj", "src/Pensees.Charon.Web.Host/"]
COPY ["src/Pensees.Charon.Web.Core/Pensees.Charon.Web.Core.csproj", "src/Pensees.Charon.Web.Core/"]
COPY ["src/Pensees.Charon.EntityFrameworkCore/Pensees.Charon.EntityFrameworkCore.csproj", "src/Pensees.Charon.EntityFrameworkCore/"]
COPY ["src/Pensees.Charon.Core/Pensees.Charon.Core.csproj", "src/Pensees.Charon.Core/"]
COPY ["src/Pensees.Charon.Application/Pensees.Charon.Application.csproj", "src/Pensees.Charon.Application/"]
RUN dotnet restore "src/Pensees.Charon.Web.Host/Pensees.Charon.Web.Host.csproj"
COPY . .
WORKDIR "/src/src/Pensees.Charon.Web.Host"
RUN dotnet build "Pensees.Charon.Web.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pensees.Charon.Web.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pensees.Charon.Web.Host.dll"]