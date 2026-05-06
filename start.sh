#!/bin/bash
cd /Users/macuser/BeautySalonAPI
dotnet build BeautySalonAPI -c Debug -v quiet 2>&1 | grep -E "error|warning|succeeded|failed"
cd /Users/macuser/BeautySalonAPI/BeautySalonAPI
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS=http://localhost:5050 \
dotnet bin/Debug/net8.0/BeautySalonAPI.dll
