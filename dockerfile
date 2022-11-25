FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY *.sln .
COPY ./SalesManagementSystem.Wasm/*.csproj ./SalesManagementSystem.Wasm/
COPY ./SalesManagementSystem.Blazor/*.csproj ./SalesManagementSystem.Blazor/
COPY ./SalesManagementSystem.Contracts/*.csproj ./SalesManagementSystem.Contracts/
COPY ./SalesManagementSystem.Server/*.csproj ./SalesManagementSystem.Server/
RUN dotnet restore
COPY ./SalesManagementSystem.Blazor/. ./SalesManagementSystem.Blazor/
COPY ./SalesManagementSystem.Wasm/. ./SalesManagementSystem.Wasm/
COPY ./SalesManagementSystem.Contracts/. ./SalesManagementSystem.Contracts/
COPY ./SalesManagementSystem.Server/. ./SalesManagementSystem.Server/
WORKDIR /src/SalesManagementSystem.Server/
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app ./
CMD ASPNETCORE_URLS=http://*:$PORT dotnet SalesManagementSystem.Server.dll