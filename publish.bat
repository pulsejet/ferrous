dotnet publish -c release -r linux-x64
dotnet ef migrations script -i -o ferrous.sql
