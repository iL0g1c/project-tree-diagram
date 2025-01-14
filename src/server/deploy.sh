#!/bin/bash
rm ./publish/*.*
dotnet publish -c Release -o ./publish
dotnet ./publish/server.dll
