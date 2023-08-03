# Frends.FTP.WriteFile
Frends tasks for writing a file to the FTP(S) server.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT) 
[![Build](https://github.com/FrendsPlatform/Frends.FTP/actions/workflows/WriteFile_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.FTP/actions)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.FTP.WriteFile)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.FTP/Frends.FTP.WriteFile|main)

# Installing

You can install the Task via frends UI Task View.

## Building

Rebuild the project

`dotnet build`

Run the Docker compose from solution root using

`docker-compose -f Frends.FTP.WriteFile.Tests/docker-compose.yml up -d`

Run tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release` 