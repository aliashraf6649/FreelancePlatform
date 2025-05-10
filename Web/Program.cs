using FreelancePlatform.Application.Common.Interfaces;
using FreelancePlatform.Application.Services;
using FreelancePlatform.Infrastructure.Data;
using FreelancePlatform.Infrastructure.Repositories;
using FreelancePlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();


// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();


builder.Services.AddScoped<IAuthService, AuthService>();


builder.Services.AddScoped<OfferService>();
// Register DbContext

// Add this to your service configuration
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IOfferService, OfferService>();
// builder.Services.AddScoped<IUserRepository, UserRepository>();
// 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
// Add health checks

// builder.Services.AddHealthChecks()
//     .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
// Add this right after var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        // Or if using migrations:
        // context.Database.Migrate();

        // Add any seed data here if needed
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");
    }
}

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// app.UseExceptionHandler("/Home/Error");
// app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

// ... other middleware ...
// app.Use(async (context, next) =>
// {
//     if (context.Request.Path.StartsWithSegments("/Job/Create") &&
//         !context.User.IsInRole("Client"))
//     {
//         context.Response.Redirect("/Account/AccessDenied");
//         return;
//     }
//     await next();
// });

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
// app.MapHealthChecks("/health");
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
