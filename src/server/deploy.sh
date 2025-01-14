#!/bin/bash
dotnet publish -c Release -o ./publish
pm2 restart Tree-Diagram-Backend
