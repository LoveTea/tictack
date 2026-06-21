FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build

WORKDIR /src

# копируем csproj с учётом контекста ./tictac
COPY tictac.csproj ./
RUN dotnet restore

# копируем всё остальное
COPY . ./

RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "tictac.dll"]