FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["QueryCV/QueryCV.fsproj", "QueryCV/"]
COPY ["cvParser/cvParser.fsproj", "cvParser/"]
RUN dotnet restore "QueryCV/QueryCV.fsproj"
COPY . .
WORKDIR "/src/QueryCV"
RUN dotnet build "QueryCV.fsproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "QueryCV.fsproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/cvParser/CV.json /app/
ENTRYPOINT ["dotnet", "QueryCV.dll"]
