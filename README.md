## Powershell CLI Instruction
1. Paste the following in your Powershell/terminal:

	```
	dotnet tool install -g dotnet-aspnet-codegenerator
	dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
	dotnet add package Microsoft.EntityFrameworkCore.Design
	dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
	dotnet add package Microsoft.AspNetCore.Identity.UI
	dotnet add package Microsoft.EntityFrameworkCore.SqlServer
	dotnet add package Microsoft.EntityFrameworkCore.Tools
	```
2. If you do not already have a DbContext file:
   1. Create a `Models` directory at the project root level. 
   2. Create a custom model cs file in `Models`. [Example.](https://learn.microsoft.com/en-us/aspnet/core/tutorials/razor-pages/model?view=aspnetcore-8.0&tabs=visual-studio)
   
> [!CAUTION]
> Be mindful of the file and object class name, make sure it does not conflict with the project name & namespace.
	 
   3. Run the following code to scaffold CRUD on that particular model, replace the \<...> with your own value:
		```
		dotnet aspnet-codegenerator razorpage -m <model-name> -dc <project-name>.Data.<pick-a-DbContext-name> -udl -outDir <pick-dir-name>/<pick-dir-name> --referenceScriptLibraries
		```
3. In your DbContext file located in the `Data` directory, pick a DbContext that you would use to store identity account information, update the class inheritance to use `IdentityDbContext` instead of `DbContext`. If necessary add `using Microsoft.AspNetCore.Identity.EntityFrameworkCore;`
4. Replace `PatientRecord.Data.PatientContext` with \<your project name\>.Data.\<your project DbContext\>, and run it in Powershell/terminal
	```
	dotnet aspnet-codegenerator identity -dc PatientRecord.Data.PatientContext
	```
5. In **Pages/Shared/_Layout.cshtml**, add this line after the \</ul> tag: `<partial name="_LoginPartial" />`
6. In **Program.cs**, after `builder.Services.AddDefaultIdentity`, add the following line to update the password hasher configuration:
	```
	builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 210000);
	```
7. In **Program.cs**, replace `builder.Services.AddRazorPages();` with the following: 
	```
	builder.Services.AddRazorPages(options =>
	{
			options.Conventions.AuthorizeFolder("/");
	});
	```
	This will enable global authorization. Unauthenticated users will be automatically redirected to the login page at all time.
8. Run the following in Powershell/terminal to update the database schema:
	```
	dotnet ef migrations add IdentityMigration
	dotnet ef database update
	```
9. Set up a SendGrid account with a valid email, create and store the API Key.
10. Create a `Services` directory at the root of project if you don't already have one.
11. Inside of `Services`, create `AuthMessageSenderOptions.cs`, copy the code in this repo.
12. Run the following in Powershell/terminal to install necessary packages:
	```
	dotnet add package SendGrid
	```
13. Append the following after `"Logging" : {...},` in `appsettings.Development.json`:
	```
	"SendGrid": {
    "SendGridKey": "<API Key from step 9>"
  }
	```
>[!WARNING]
>In production, sensitive data like this should not be stored in appsettings.json, consider other options like Azure Key Vault.
14. Inside of `Services`, create `EmailSender.cs`, copy the code in this repo.
15. Inside of `Program.cs`, add these on the top: 
	```
	using Microsoft.AspNetCore.Identity.UI.Services;
	using WebPWrecover.Services;
	```
	And these before `var app = builder.Build();`:
	```
	builder.Services.AddTransient<IEmailSender, EmailSender>();
	builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration.GetSection("SendGrid"));
	```
16. In `Areas/Identity/Pages/Account/RegisterConfirmation.cshtml.cs`, the file created by aspnet-codegenerator, change `DisplayConfirmAccountLink = true;` on line 63 to `DisplayConfirmAccountLink = false;`.
17. Don't forget to update the message being sent to avoid being marked as junk/spam.
18. 