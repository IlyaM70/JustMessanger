using MessageService;
using MessageService.Data;
using MessageService.Hubs;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<MessageDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHttpClient<AuthorizationClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7135");
});

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7097")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(); // Call this *before* app.UseAuthorization()

app.UseAuthorization();

app.MapControllers();
app.MapHub<MessagesHub>("/messagesHub");

app.Run();
