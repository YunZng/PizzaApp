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
13. Append the following after `"Logging" : {...},` in `appsettings.Development.json`, replace `<...>` with your information:
	```
	"SendGrid": {
	    "SendGridKey": "<SendGrid API key>",
	    "SendGridEmail": "<SendGrid email>",
	    "AppOwnerEmail": "<App owner email>"
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
	These will allow you to access the values from step 13 using DI(dependency injection). You can use these values to configure which SendGrid email is used to send confirmation email.
	In this repo, line 35 in `Services/EmailSender.cs` and line 141 in `Areas/Identity/Pages/Account/Register.cshtml.cs` both uses those custom values.
16. In `Areas/Identity/Pages/Account/RegisterConfirmation.cshtml.cs`, the file created by aspnet-codegenerator, change `DisplayConfirmAccountLink = true;` on line 63 to `DisplayConfirmAccountLink = false;`.
17. Don't forget to update the message being sent to avoid being marked as junk/spam. (line 141 in `Areas/Identity/Pages/Account/Register.cshtml.cs`)

> Up to this point, the app supports basic CRUD operation on a list of Pizza, basic authentication with email and password, and the ability to send account confirmation to specified email address.
---
> The following content will cover more advanced topics such as custom identity user, and role-based authorization.

### Overview of the app
The app comes with a predefined app owner email address. The first account to register will be the app owner, and a confirmation email will be sent to the app owner's email address. After that, all registeration requests will be considered the admin role, and the app owner will receive confirmation email of those requests.

Both the app owner and admins have the ability to perform CRUD operations on accounts. In this particular implementation, however, the app owner can only create admin accounts, but can view/edit/delete every account including staffs. Admins, on the other hand, will only be authorized to perform CRUD on the staff accounts that they created.

### Custom identity user
Sometimes, you may want to store more information on an identity user, such as assigning a user to a manager. In the Pizza app, I added a custom field `CreatedBy` that represents the creator/manager of the account. 

For example, the `CreatedBy` field of the app owner account john@email.com will be "self". The `CreatedBy` field of all admins will be "john@email.com". If an admin account doe@email.com created a staff account, then that staff account's `CreatedBy` field will be "doe@email.com". 

This app also uses the `CreatedBy` field as a resource group for the purpose of demo. So accounts can only access data within the same `CreatedBy` group. 

### Role-based Authorization
Normally, following the .NET Identity tutorial, you will only encounter the `UserManager` class. If you want to assign role to users, we need to use both the `UserManager` and the `RoleManager` class. The first step is to create a role and store it in a separate table with `RoleManager`. Then roles are assigned to users with `UserManager`. Each user may have multiple roles, and each role may be assigned to multiple users. 

Sometimes you may want to restrict access for certain roles. To do this, you need to add the `[Authorize]` attribute above a page model class. We can refine this attribute by passing the specified roles as comma separated list like `[Authorize(Roles = "Admin,Owner")]`. This will only give role "Admin" or "Owner" access to the page and functions. Within these page models, we will have to manually extract the roles of the logged in user with `UserManager` to controll the operation. 
