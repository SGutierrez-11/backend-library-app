using backend_library_app.Context;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configurar Firebase
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("./library-app-d3e16-firebase-adminsdk-a8fnv-a0fec7462e.json")
});

// Add services to the container.
//Variable to connection DB
var connectionString = builder.Configuration.GetConnectionString("Connection");
//Register services to Connection
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddCors(p => p.AddPolicy("corspolicy", build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

string glusterMountPoint = "/data/gluster/subdir1";
if (!Directory.Exists(glusterMountPoint))
{
    Directory.CreateDirectory(glusterMountPoint);
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("corspolicy");
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
