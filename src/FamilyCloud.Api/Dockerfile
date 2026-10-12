FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /var/api
COPY . .
RUN dotnet publish src/FamilyCloud.Api/FamilyCloud.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "FamilyCloud.Api.dll" ]