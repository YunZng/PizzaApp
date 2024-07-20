using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using PizzaApp.Areas.Identity.Data;
using PizzaApp.Services;
using WebPWrecover.Services;

var builder = WebApplication.CreateBuilder(args);

// Enables global authroization, must authenticate to access any page.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
});

builder.Services.AddDbContext<PizzaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING") ?? throw new InvalidOperationException("Connection string 'AZURE_SQL_CONNECTIONSTRING' not found."), builder => builder.EnableRetryOnFailure(3)));

builder.Services.AddDefaultIdentity<PizzaIdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<PizzaDbContext>();

// Configure PasswordHasher, compatibility mode defaults to 'ASP.NET Identity version 3', which uses SHA512 with 100,000 iterations.
builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 210000);

// Set up email sender
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration.GetSection("SMTP"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<InvitationService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
