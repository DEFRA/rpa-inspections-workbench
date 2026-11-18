#!/bin/sh
# exec replaces this shell process so dotnet receives signals directly, not the shell.
exec dotnet watch --project ./RPA.Inspections.Workbench run --urls "http://*:${PORT}"
