var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();     
builder.Services.AddSingleton<SchedulerDemo.Services.FileLoggerService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();
app.Run();
