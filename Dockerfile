

# Stage 1
FROM microsoft/dotnet:2.2-sdk AS builder
WORKDIR /source

# caches restore result by copying csproj file separately 
COPY . .
WORKDIR /source/MarsRoverPics/
RUN dotnet restore

# copies the rest of your code
RUN dotnet publish --output /app/ --configuration Release

#run tests
WORKDIR /source/MarsRoverPics.Tests/
RUN dotnet restore
RUN dotnet test -c Release /p:CollectCoverage=true /p:Threshold=80 /p:ThresholdType=Line /p:exclude=\"[xunit.*]*,[*.Views]*\"

# Stage 2
FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app .
# setup user and urls for diff port 
ENV ASPNETCORE_URLS=http://*:5300
EXPOSE 5300
RUN adduser --no-create-home --disabled-password --gecos "" dotnet && chown -R dotnet:dotnet /app 
USER dotnet
ENTRYPOINT ["dotnet", "MarsRoverPics.dll"]
