#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["test/Proge.Teams.Edu.ItalianUniversity.ConsoleApp/Proge.Teams.Edu.ItalianUniversity.ConsoleApp.csproj", "test/Proge.Teams.Edu.ItalianUniversity.ConsoleApp/"]
COPY ["test/Proge.Teams.Edu.ItalianUniversity/Proge.Teams.Edu.ItalianUniversity.csproj", "test/Proge.Teams.Edu.ItalianUniversity/"]
COPY ["src/Proge.Teams.Edu.GraphApi/Proge.Teams.Edu.GraphApi.csproj", "src/Proge.Teams.Edu.GraphApi/"]
COPY ["src/Proge.Teams.Edu.Abstraction/Proge.Teams.Edu.Abstraction.csproj", "src/Proge.Teams.Edu.Abstraction/"]
COPY ["src/Proge.Teams.Edu.Abstraction/nuget.config", "src/Proge.Teams.Edu.Abstraction/"]
COPY ["src/Proge.Teams.Edu.DAL/Proge.Teams.Edu.DAL.csproj", "src/Proge.Teams.Edu.DAL/"]
RUN dotnet restore "test/Proge.Teams.Edu.ItalianUniversity.ConsoleApp/Proge.Teams.Edu.ItalianUniversity.ConsoleApp.csproj"
COPY . .
WORKDIR "/src/test/Proge.Teams.Edu.ItalianUniversity.ConsoleApp"
RUN dotnet build "Proge.Teams.Edu.ItalianUniversity.ConsoleApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Proge.Teams.Edu.ItalianUniversity.ConsoleApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Proge.Teams.Edu.ItalianUniversity.ConsoleApp.dll"]