using Microsoft.EntityFrameworkCore;
using shrt.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApiDbContext>(options => options.UseSqlite(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/shorturl", async (UrlDto url, ApiDbContext db, HttpContext ctx) =>
{
    // Validate url
    if (!Uri.TryCreate(url.Url, UriKind.Absolute, out var inputUrl))
        return Results.BadRequest("Invalid url has been provided");

    // Create a shortened version of the url
    var random = new Random();
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890@az";
    var randomStr = new string(Enumerable.Repeat(chars, 8)
        .Select(x => x[random.Next(x.Length)]).ToArray());

    // Create db object
    var sUrl = new UrlManagement()
    {
        Url = url.Url,
        ShortUrl = randomStr
    };

    // Save mapping to db
    db.Urls.Add(sUrl);
    db.SaveChangesAsync();

    // Response
    var result = $"{ctx.Request.Scheme}://{ctx.Request.Host}/{sUrl.ShortUrl}";
    return Results.Ok(new UrlShortResponseDto()
    {
        Url = result
    });
});

app.MapFallback(async (ApiDbContext db, HttpContext ctx) =>
{
    var path = ctx.Request.Path.ToUriComponent().Trim('/');
    var urlMatch = await db.Urls.FirstOrDefaultAsync(x => x.ShortUrl.Trim() == path.Trim());

    if (urlMatch == null)
    {
        return Results.BadRequest("Invalid request");
    }

    return Results.Redirect(urlMatch.Url);
    });

app.Run();

class ApiDbContext : DbContext
{
    public virtual DbSet<UrlManagement> Urls {get; set;}

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
        
    }
}