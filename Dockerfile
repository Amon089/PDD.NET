FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Habitto.sln .
COPY src/Habitto.Domain/Habitto.Domain.csproj src/Habitto.Domain/
COPY src/Habitto.Application/Habitto.Application.csproj src/Habitto.Application/
COPY src/Habitto.Infrastructure/Habitto.Infrastructure.csproj src/Habitto.Infrastructure/
COPY src/Habitto.Api/Habitto.Api.csproj src/Habitto.Api/

RUN dotnet restore Habitto.sln

COPY src/ src/
RUN dotnet publish src/Habitto.Api/Habitto.Api.csproj -c Release -o /app/publish 

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Habitto.Api.dll"]
