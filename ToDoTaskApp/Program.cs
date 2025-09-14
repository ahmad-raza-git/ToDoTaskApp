using Azure.Storage.Blobs;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton(x =>
{
    var config = builder.Configuration.GetSection("AzureStorage");
    return new BlobContainerClient(config["ConnectionString"], config["ContainerName"]);
});

// Add Hangfire services
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

builder.Services.AddSingleton<BlobService>();
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

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

// Hangfire Dashboard (optional but useful)
app.UseHangfireDashboard("/hangfire");
// 3. Register recurring jobs AFTER app is built
RecurringJob.AddOrUpdate<ReminderJob>(
    "daily-task-reminder",
    j => j.SendDailyReminders(),
    "1 0 * * *" // runs daily at 00:01 server time
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TodoTasks}/{action=Index}/{id?}");

app.Run();
