using Microsoft.EntityFrameworkCore;
using POLLS.Data;
using POLLS.Interfaces;
using POLLS.Repositories;
using POLLS.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IPollRepository, PollRepository>();
builder.Services.AddScoped<IPollService, PollService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6380")
);

builder.Services.AddDbContext<PollsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

app.UseStaticFiles();
app.UseWebSockets();

app.MapControllers();

Console.WriteLine("HTTP Server running! Verifique as URLs no seu terminal.");
app.Run();