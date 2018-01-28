dotnet publish -c release
cd bin
cd release
cd netcoreapp2.0
copy appsettings.Development.json publish
copy appsettings.json publish
7z a publish.zip ./publish/*
7z a Ferrous.zip publish.zip aws-windows-deployment-manifest.json
del publish.zip
cd ../../../