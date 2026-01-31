FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebApp/WebApp.csproj", "WebApp/"]
COPY ["App.BLL/App.BLL.csproj", "App.BLL/"]
COPY ["App.BLL.Contracts/App.BLL.Contracts.csproj", "App.BLL.Contracts/"]
COPY ["App.Domain/App.Domain.csproj", "App.Domain/"]
COPY ["App.Common/App.Common.csproj", "App.Common/"]
COPY ["App.DAL/App.DAL.csproj", "App.DAL/"]
COPY ["App.DAL.Contracts/App.DAL.Contracts.csproj", "App.DAL.Contracts/"]
COPY ["App.DTO/App.DTO.csproj", "App.DTO/"]
COPY ["App.Validation/App.Validation.csproj", "App.Validation/"]
COPY ["Base.Domain/Base.Domain.csproj", "Base.Domain/"]

RUN dotnet restore "WebApp/WebApp.csproj"
COPY . .
WORKDIR "/src/WebApp"
RUN dotnet build "WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApp.dll"]
