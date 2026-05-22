FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY . .

RUN dotnet restore SteamPrefill.sln

# IMPORTANT: self-contained Linux binary build
RUN dotnet publish SteamPrefill/SteamPrefill.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -o /out

FROM ubuntu:20.04

RUN apt-get update && apt-get install -y \
    ca-certificates \
    libncursesw5 \
    curl jq unzip wget \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /usr/bin

COPY --from=build /out .

RUN chmod +x SteamPrefill

ENTRYPOINT ["./SteamPrefill"]
