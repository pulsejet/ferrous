# Stage 1
FROM microsoft/dotnet:2.1.300-sdk-stretch AS builder
WORKDIR /source

# caches restore result by copying csproj file separately
COPY ./*.csproj .
RUN dotnet restore

# copies the rest of your code
COPY . .
RUN cd ClientApp && npm install && npm run lint && cd ..
RUN dotnet publish --output /app/ --configuration Release

# Stage 2 (enable if continuous integration is necessary)
# FROM microsoft/aspnetcore
# WORKDIR /app
# COPY --from=builder /app .
# ENTRYPOINT ["dotnet", "aspdocker.dll"]