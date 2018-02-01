cd ..
cd bin
cd release
cd netcoreapp2.0
copy appsettings.Development.json ubuntu.16.04-x64\publish
copy appsettings.json ubuntu.16.04-x64\publish
7z a publish.zip ./ubuntu.16.04-x64/publish/*
cd ../../../