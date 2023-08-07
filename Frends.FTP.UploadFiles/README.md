# Frends.FTP.UploadFiles

Upload files to FTP(S) server.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Build](https://github.com/FrendsPlatform/Frends.FTP/actions/workflows/UploadFiles_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.FTP/actions)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.FTP/Frends.FTP.UploadFiles|main)

## Installing

You can install the Task via Frends UI Task View.

## Building

### Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.FTP.git`

### Build the project

`dotnet build`

### Run tests

Run the Docker compose from solution root using 

`docker-compose -f Frends.FTP.UploadFiles.Tests/docker-compose.yml up -d`

Run the tests

`dotnet test`

### Create a NuGet package

`dotnet pack --configuration Release`