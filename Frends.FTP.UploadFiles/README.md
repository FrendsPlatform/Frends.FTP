# Frends.Regex.IsMatch

[![Frends.Regex.IsMatch Main](https://github.com/FrendsPlatform/Frends.Regex/actions/workflows/IsMatch_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.Regex/actions/workflows/IsMatch_build_and_test_on_main.yml)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.Regex.IsMatch?label=NuGet)
 ![GitHub](https://img.shields.io/github/license/FrendsPlatform/Frends.Regex?label=License)
 ![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.Regex|Frends.Regex.IsMatch|main)

Returns a boolean depicting if the input text matches with the specified regular expression.

## Installing

You can install the Task via frends UI Task View or you can find the NuGet package from the following NuGet feed
https://www.myget.org/F/frends-tasks/api/v2.

## Properties

| Property          | Type     | Description             | Example     |
| ----------------- | -------- | ----------------------- | ----------- |
| InputText         | `string` | Text to test with regex | `foo`       |
| RegularExpression | `string` | The regular expression  | `[f].*?[o]` |

## Returns

A result object IsMatchResult with parameters:

| Property | Type   | Description                                          | Example |
| -------- | ------ | ---------------------------------------------------- | ------- |
| IsMatch  | `bool` | `true` if the text is a regex match, `false` if not. | `true`  |

Usage:
To fetch result use syntax:

`#result.IsMatch`

## Building

Clone a copy of the repository

`git clone https://github.com/FrendsPlatform/Frends.Regex.git`

Rebuild the project

`dotnet build`

Run tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release`
