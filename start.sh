#!/bin/bash
exec dotnet HelpDesk/out/HelpDesk.dll --urls http://+:${PORT:-5000}
