## Powershell CLI Version (Yulun recommend)
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

7. Run the following in Powershell/terminal to update the database schema:
	```
	dotnet ef migrations add IdentityMigration
	dotnet ef database update
	```


## Visual Studio Version
1.	From Solution Explorer, right-click on the project > Add > New Scaffolded Item.
2.	From the left pane of the Add New Scaffolded Item dialog, select Identity. Select Identity in the center pane. Select the Add button.
3.	In the Add Identity dialog, select the options you want.
4.	For the data context (DbContext class): Select your data context class.
5.	To create a data context and possibly create a new user class for Identity, select the + button. Accept the default value or specify a class (for example, Contoso.Data.ApplicationDbContext for a company named "Contoso"). To create a new user class, select the + button for User class and specify the class (for example, ContosoUser for a company named "Contoso"). You can also use this function to customize your user account information.
6.	Select the Add button to run the scaffolder.
7.	In Pages/Shared/_Layout.cshtml, add this line after the </ul> tag: <partial name="_LoginPartial" />