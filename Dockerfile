# Stage 1
FROM microsoft/aspnetcore-build AS builder
WORKDIR /source

# caches restore result by copying csproj file separately
COPY ./Ferrous/*.csproj .
RUN dotnet restore

# copies the rest of your code
COPY ./Ferrous .
RUN dotnet publish --output /app/ --configuration Release

# Stage 2 (enable if continuous integration is necessary)
# FROM microsoft/aspnetcore
# WORKDIR /app
# COPY --from=builder /app .
# ENTRYPOINT ["dotnet", "aspdocker.dll"]