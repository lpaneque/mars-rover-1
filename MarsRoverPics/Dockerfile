FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY MarsRoverPics/MarsRoverPics.csproj MarsRoverPics/
RUN dotnet restore MarsRoverPics/MarsRoverPics.csproj
COPY . .
WORKDIR /src/MarsRoverPics
RUN dotnet build MarsRoverPics.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish MarsRoverPics.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MarsRoverPics.dll"]
