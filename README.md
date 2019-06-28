# MarsRoverPics

This web application will launch a single page that will show one random picture from the NASA Rover API. The application will randomly select a date from the configured ones (in appsettings.json) and will pick a rover and photo randomly. Every refresh of the home page will download a new photo randomly again, if the photo selected is the same, it is just returned and not re-downloaded.

The application is built using ASPNET CORE MVC, C#. The unit test project is built using xUnit, Moq, Coverlet and AutoFixture. All dependencies are nuget packages.

Images will be stored locally after first download and calls to NASA Rover API will be cached in memory.

Configuration is donde via appsettings.json file.

NOTES
- If dates in the appsettings.json file are incorrect, they will be ignored, if no dates are valid, the application will fail.
- The appsettings.json file contains a section where the url and api key for the NASA rover API is configured.
- In the case of a fatal error downloading the image from NASA servers - ONLY in that case - the application will use the original link from the API.
- If the given combination of rover and date does not yield any photos, a "not found" image will be shown instead.

## Requirements

This solution was implemented in C# and NETCORE and can be built from Visual Studio or just from a system with Docker (any platform)

### with Visual Studio on Windows Machine

- Visual Studio 2017 v15.9.3 with C#
- DotNetCore SDK 2.2
- Docker for Windows (with Linux as target)

### with Docker-only system
- Docker on any OS


## How to Build

### Visual Studio

- To run in normal mode, set the project named "MarsRoverPics" as startup project, then press CONTROL+F5. A browser will open with the application running.
- To run in a docker container from within Visual Studio, select the project named "docker-compose" as startup project and CONTROL+F5. A browser will open with the application now running in a docker container

NOTES: Even the machine with Visual Studio does not have docker, you can still build and run only the web and unit test projects.


### Docker only

From the root directory of the solution, run:

```sh
$ docker build . -t rover
```

After the image is built, the run it with:

```sh
$ docker run -p 8887:5300 rover
```

Then open a browser on http://localhost:8887

## Unit Testing & Coverage

The application includes unit tests with over 80% coverage and the build process in the Dockerfile at the root of the solution enforces that:
- All Unit tests pass
- Coverage (per line) is above 80 %
