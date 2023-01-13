using System;
using gamecrew.Helpers;
using gamecrew.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);                    

// Interesting Note I found from dotnet 7 docs:
// WebApplication.CreateBuilder initializes a 
// new instance of the WebApplicationBuilder class 
// with preconfigured defaults. The initialized WebApplicationBuilder 
// (builder) provides default configuration and calls AddUserSecrets when the EnvironmentName is Development
// https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-7.0&tabs=windows#register-the-user-secrets-configuration-source

#region AppSettings
AppSettings settings = new AppSettings{
    ConnectionString = builder.Configuration["PlayerGroupsDatabaseSettings:ConnectionString"],
    DatabaseName = builder.Configuration["PlayerGroupsDatabaseSettings:DatabaseName"],
    Key = builder.Configuration["Tokens:Key"],
    Exp = Convert.ToInt32(builder.Configuration["Tokens:Exp"]),
};
builder.Services.AddSingleton(settings);
#endregion AppSettings

#region MongoDB Collection Services
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IGroupServices, GroupService>();
#endregion MongoDB Collection Services

#region Context Accesor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
#endregion Context Accesor

#region Sessions
builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".gamecrew.sessh";
                options.IdleTimeout = TimeSpan.FromDays(15);
                options.Cookie.IsEssential = true;
                options.Cookie.MaxAge = TimeSpan.FromDays(15);
            });
#endregion Sessions

builder.Services.AddControllersWithViews();


var app = builder.Build();


app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
