FROM mcr.microsoft.com/dotnet/sdk:10.0.403-noble AS build
WORKDIR /src
COPY RPA.Inspections.Workbench/RPA.Inspections.Workbench.csproj RPA.Inspections.Workbench/

# Optional non-secret package source (for example an internal feed mirror).
ARG NUGET_SOURCE
RUN if [ -n "$NUGET_SOURCE" ]; then \
        dotnet restore RPA.Inspections.Workbench/RPA.Inspections.Workbench.csproj --source "$NUGET_SOURCE"; \
    else \
        dotnet restore RPA.Inspections.Workbench/RPA.Inspections.Workbench.csproj; \
    fi

COPY RPA.Inspections.Workbench/ RPA.Inspections.Workbench/
RUN dotnet publish RPA.Inspections.Workbench/RPA.Inspections.Workbench.csproj \
    -c Release --no-restore -o /out /p:UseAppHost=false

# Ubuntu includes ICU: en-GB dates and number formats must not use invariant globalization.
FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-noble AS final
WORKDIR /app
COPY --from=build /out/ ./
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "RPA.Inspections.Workbench.dll"]