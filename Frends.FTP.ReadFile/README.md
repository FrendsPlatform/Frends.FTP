# Frends.FTP.ReadFile
Frends tasks for reading a file from FTP(S) server.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT) 
[![Build](https://github.com/FrendsPlatform/Frends.FTP/actions/workflows/ReadFile_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.FTP/actions)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.FTP.ReadFile)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.FTP/Frends.FTP.ReadFile|main)

# Installing

You can install the Task via frends UI Task View.

## Building

Rebuild the project

`dotnet build`

Run the Docker compose from solution root using

`docker-compose -f Frends.FTP.ReadFile.Tests/docker-compose.yml up -d`

Run tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release` 