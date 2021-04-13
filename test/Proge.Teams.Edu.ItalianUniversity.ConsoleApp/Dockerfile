FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
USER nobody
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Proge.Teams.Edu.ItalianUniversity.ConsoleApp/Proge.Teams.Edu.ItalianUniversity.ConsoleApp.csproj", "Proge.Teams.Edu.ItalianUniversity.ConsoleApp/"]
COPY ["Proge.Teams.Edu.ItalianUniversity/Proge.Teams.Edu.ItalianUniversity.csproj", "Proge.Teams.Edu.ItalianUniversity/"]
COPY ["Proge.Teams.Edu.Abstraction/Proge.Teams.Edu.Abstraction.csproj", "Proge.Teams.Edu.Abstraction/"]
COPY ["Proge.Teams.Edu.DAL/Proge.Teams.Edu.DAL.csproj", "Proge.Teams.Edu.DAL/"]
COPY ["Proge.Teams.Edu.GraphApi/Proge.Teams.Edu.GraphApi.csproj", "Proge.Teams.Edu.GraphApi/"]
RUN dotnet restore "Proge.Teams.Edu.ItalianUniversity.ConsoleApp/Proge.Teams.Edu.ItalianUniversity.ConsoleApp.csproj"
COPY . .
WORKDIR "/src/Proge.Teams.Edu.ItalianUniversity.ConsoleApp"
RUN dotnet build "Proge.Teams.Edu.ItalianUniversity.ConsoleApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Proge.Teams.Edu.ItalianUniversity.ConsoleApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Proge.Teams.Edu.ItalianUniversity.ConsoleApp.dll"]