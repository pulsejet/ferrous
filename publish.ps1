dotnet publish -c release -r linux-x64 -o built
dotnet ef migrations script -i -o ferrous.sql
Add-Type -A System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory('built', 'publish.zip')
Compress-Archive -Path LICENSE,ferrous.sql -DestinationPath publish.zip -Update
