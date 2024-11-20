using AdminSignin.Client.Pages;
using AdminSignin.Components;
using Supabase.Interfaces;
using Supabase;
using Microsoft.AspNetCore.Authentication.Cookies;
using AdminSignin.Components.Service;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using AdminSignin.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.JSInterop;
using AdminSignin.Components.Services;
using AdminSignin.Components.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddHttpClient<AuthenticationService>(client =>
{
    client.BaseAddress = new Uri("https://jqkvwczfbdcspamtunoi.supabase.co");
});

builder.Services.AddControllers();

var mongoConnectionString = builder.Configuration.GetSection("MongoDB:ConnectionString").Value;
var mongoDatabaseName = builder.Configuration.GetSection("MongoDB:DatabaseName").Value;

builder.Services.AddScoped<IMongoClient>(sp =>
    new MongoClient(mongoConnectionString));
builder.Services.AddScoped<ResumeService>();
builder.Services.AddScoped<FAQService>();

builder.Services.AddSingleton<TestimonyService>();
builder.Services.AddSingleton<SuccessStoryService>();

builder.Services.AddAuthentication();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login"; // Adjust this path to match your login route
});


var supabaseConfiguration = builder.Configuration
    .GetSection("Supabase")
    .Get<SupabaseConfiguration>();

builder.Services.AddScoped<Supabase.Client>(provider =>
{
    var url = builder.Configuration["Supabase:Url"];
    var key = builder.Configuration["Supabase:Key"];
    return new Supabase.Client(url, key);
});



// Register Supabase client
builder.Services.AddScoped<Supabase.Client>(provider =>
    new Supabase.Client(supabaseConfiguration.Url, supabaseConfiguration.Key));

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddScoped<MongoDbService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddMudServices();
builder.Services.AddLogging();
builder.Services.AddAuthentication();
builder.Services.AddHttpClient<ApiService>();
builder.Services.AddScoped<BreadcrumbTracker>();
builder.Services.AddHttpClient<AuthenticationService>();

builder.Services.AddScoped<DatabaseService>();

builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<UserAuditService>();
builder.Services.AddHttpClient();

builder.Services.AddMudServices();

// Read email settings from configuration
var emailSettings = builder.Configuration.GetSection("EmailSettings");
var smtpServer = emailSettings["SmtpServer"];
var smtpPort = int.Parse(emailSettings["SmtpPort"]);
var fromAddress = emailSettings["FromAddress"];
var smtpUser = emailSettings["SmtpUser"];
var smtpPass = emailSettings["SmtpPass"];





builder.Services.AddScoped<AdminAccessModels>();
// Register EmailService
// Register EmailService
builder.Services.AddSingleton(new AdminSignin.Services.EmailService(smtpServer, smtpPort, fromAddress, smtpUser, smtpPass));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenResponse>();
builder.Services.AddScoped<TokenService>();

builder.Services.AddScoped<DepartmentService>();

builder.Services.AddScoped<AdminAccessService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Enable authentication and authorization middleware

app.UseAuthentication();
app.UseAuthorization();



app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(AdminSignin.Client._Imports).Assembly);

app.Run();
