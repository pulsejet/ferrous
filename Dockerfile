# Stage 1
FROM microsoft/aspnetcore-build:2.1.300-preview1 AS builder
WORKDIR /source

# caches restore result by copying csproj file separately
COPY ./Ferrous/*.csproj .
RUN dotnet restore

# copies the rest of your code
COPY ./Ferrous .
RUN cd ClientApp && npm install && npm run lint && cd ..
RUN dotnet publish --output /app/ --configuration Release

# Stage 2 (enable if continuous integration is necessary)
# FROM microsoft/aspnetcore
# WORKDIR /app
# COPY --from=builder /app .
# ENTRYPOINT ["dotnet", "aspdocker.dll"]