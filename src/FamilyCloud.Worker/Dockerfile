FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /var/worker
COPY . .
RUN dotnet publish src/FamilyCloud.Worker/FamilyCloud.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "FamilyCloud.Worker.dll" ]