using EventBooking.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Services
// -----------------------------

// Configure form options
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

// Configure database connection with fallback for local development
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Render provides DATABASE_URL (postgres://...)
if (string.IsNullOrEmpty(connectionString))
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrEmpty(databaseUrl))
    {
        // Convert postgres:// to Npgsql format
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        connectionString =
            $"Host={uri.Host};" +
            $"Port={uri.Port};" +
            $"Database={uri.AbsolutePath.TrimStart('/')};" +
            $"Username={userInfo[0]};" +
            $"Password={userInfo[1]};" +
            $"SSL Mode=Require;Trust Server Certificate=true";
    }
    else
    {
        // Local fallback
        connectionString =
            "Host=localhost;Port=5432;Database=EventBookingDb;Username=postgres;Password=gabu";
    }
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Authentication + JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS policy (Frontend: Next.js)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000") // Next.js dev server
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// -----------------------------
// Apply Database Migrations (Production & Development)
// -----------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("✅ Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration error: {ex.Message}");
    }
}

// -----------------------------
// Middleware
// -----------------------------

// Enable Swagger in ALL environments (for testing)
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

// Use HTTPS redirection (optional - Render handles SSL)
// app.UseHttpsRedirection();

// Use CORS (must be before Authentication/Controllers)
app.UseCors("AllowFrontend");

// Use Authentication + Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add a health check endpoint
app.MapGet("/", () => Results.Ok(new
{
    status = "healthy",
    service = "Event Booking API",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow
}));

app.Run();