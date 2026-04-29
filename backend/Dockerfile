FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY . .
RUN echo "cache bust 1" && ls -la && find . -name "*.sln" && exit 1