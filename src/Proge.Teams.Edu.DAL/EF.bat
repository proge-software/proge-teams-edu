dotnet ef migrations add Init --startup-project ./ --project ../Proge.Teams.Edu.DAL/ -v
dotnet ef database update --startup-project ./ --project ../Proge.Teams.Edu.DAL/ -v