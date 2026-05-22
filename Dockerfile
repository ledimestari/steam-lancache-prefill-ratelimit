# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files first (better layer caching)
COPY . .

# Restore + publish
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish --no-restore

# =========================
# Runtime stage
# =========================
FROM ubuntu:20.04

ENV DEBIAN_FRONTEND=noninteractive

RUN apt update && apt install -y \
    ca-certificates \
    libncursesw5 \
    locales \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy compiled output from build stage
COPY --from=build /app/publish .

# Ensure binary is executable (if native executable is produced)
RUN chmod +x SteamPrefill || true

ENTRYPOINT ["./SteamPrefill"]
