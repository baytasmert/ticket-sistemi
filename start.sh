#!/bin/bash
cd HelpDesk
dotnet build --configuration Release
dotnet run --configuration Release --urls http://+:${PORT:-5065}
