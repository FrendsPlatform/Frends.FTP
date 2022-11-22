# Frends.FTP.DownloadFiles
Downloads files from a FTP(S) server.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Build](https://github.com/FrendsPlatform/Frends.FTP/actions/workflows/DownloadFiles_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.FTP/actions)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.FTP.DownloadFiles)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.FTP/Frends.FTP.DownloadFiles|main)

## Installing

You can install the Task via frends UI Task View or you can find the NuGet package from the following NuGet feed
https://www.myget.org/F/frends-tasks/api/v2.

## Building

### Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.FTP.git`

### Build the project

`dotnet build`

### Run tests

Run the Docker compose from solution root using

`docker-compose -f Frends.FTP.DownloadFiles.Tests/docker-compose.yml up`

Run the tests

`dotnet test`

### Create a NuGet package

`dotnet pack --configuration Release`
