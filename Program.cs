using Microsoft.EntityFrameworkCore;
using shrt.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

class ApiDbContext : DbContext
{
    public virtual DbSet<UrlManagement> Urls {get; set;}

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
        
    }
}