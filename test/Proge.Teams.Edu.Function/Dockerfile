#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Proge.Teams.Edu.Function/Proge.Teams.Edu.Function.csproj", "Proge.Teams.Edu.Function/"]
COPY ["Proge.Teams.Edu.DAL/Proge.Teams.Edu.DAL.csproj", "Proge.Teams.Edu.DAL/"]
COPY ["Proge.Teams.Edu.Abstraction/Proge.Teams.Edu.Abstraction.csproj", "Proge.Teams.Edu.Abstraction/"]
COPY ["Proge.Teams.Edu.TeamsDashboard/Proge.Teams.Edu.TeamsDashboard.csproj", "Proge.Teams.Edu.TeamsDashboard/"]
COPY ["Proge.Teams.Edu.GraphApi/Proge.Teams.Edu.GraphApi.csproj", "Proge.Teams.Edu.GraphApi/"]
RUN dotnet restore "Proge.Teams.Edu.Function/Proge.Teams.Edu.Function.csproj"
COPY . .
WORKDIR "/src/Proge.Teams.Edu.Function"
RUN dotnet build "Proge.Teams.Edu.Function.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Proge.Teams.Edu.Function.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true