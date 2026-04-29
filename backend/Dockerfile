FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore Playspot.API/Playspot.API.csproj
RUN dotnet publish Playspot.API/Playspot.API.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE $PORT

ENTRYPOINT ["dotnet", "Playspot.API.dll"]
