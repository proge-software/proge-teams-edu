#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/Proge.Teams.Edu.TeamsDashboard/nuget.config", "src/Proge.Teams.Edu.TeamsDashboard/"]
COPY ["src/Proge.Teams.Edu.Abstraction/nuget.config", "src/Proge.Teams.Edu.Abstraction/"]
COPY ["test/Proge.Teams.Edu.Function/Proge.Teams.Edu.Function.csproj", "test/Proge.Teams.Edu.Function/"]
COPY ["src/Proge.Teams.Edu.TeamsDashboard/Proge.Teams.Edu.TeamsDashboard.csproj", "src/Proge.Teams.Edu.TeamsDashboard/"]
COPY ["src/Proge.Teams.Edu.Abstraction/Proge.Teams.Edu.Abstraction.csproj", "src/Proge.Teams.Edu.Abstraction/"]
COPY ["src/Proge.Teams.Edu.DAL/Proge.Teams.Edu.DAL.csproj", "src/Proge.Teams.Edu.DAL/"]
COPY ["src/Proge.Teams.Edu.GraphApi/Proge.Teams.Edu.GraphApi.csproj", "src/Proge.Teams.Edu.GraphApi/"]
COPY ["src/Proge.Teams.Edu.Web/Proge.Teams.Edu.Web.csproj", "src/Proge.Teams.Edu.Web/"]
RUN dotnet restore "test/Proge.Teams.Edu.Function/Proge.Teams.Edu.Function.csproj"
COPY . .
WORKDIR "/src/test/Proge.Teams.Edu.Function"
RUN dotnet build "Proge.Teams.Edu.Function.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Proge.Teams.Edu.Function.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true