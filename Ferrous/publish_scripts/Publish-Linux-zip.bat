cd ..
cd bin
cd release
cd netcoreapp2.0
copy appsettings.Development.json linux-x64\publish
copy appsettings.json linux-x64\publish
7z a publish.zip ./linux-x64/publish/*
cd ../../../