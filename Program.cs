using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StoreRewards.Data;
using StoreRewards.Services.Email;
using StoreRewards.Services;
using System.Text;
using Serilog;
using StoreRewards.Services.Excel;

var builder = WebApplication.CreateBuilder(args);


var logDirectory = "C:/General-Data/API-Server-Logs";
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    //.MinimumLevel.Debug() // Set the minimum log level
    .WriteTo.File(Path.Combine(logDirectory, "log-.txt"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Load environment variables from the .env file
Env.Load();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add authentication services (e.g., JWT)
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        //options.Authority = "https://localhost:7208";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_Key") ?? "")),
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_Issuer"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_Audience"),
        };
    });
// Add authorization services
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Marketer"));
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("Buyer"));
});
//-------
builder.Services.AddSingleton<AuthService>();
//-------

builder.Services.AddSingleton<IMailService, MailService>();
builder.Services.AddSingleton<IUploadExcelService, UploadExcelService>();
//-------
builder.Services.AddScoped<ImageService>();
builder.Services.AddHttpContextAccessor();

// Register the encoding provider for older encodings
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:3000", "https://munira.vercel.app", "https://mun4bus.com")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});


var app = builder.Build();

// Serve static files (images)
app.UseStaticFiles();  

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
/*
if (app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        //c.SwaggerEndpoint("swagger/v1/swagger.json", "API V1.0"); 
        c.SwaggerEndpoint("https://mun4bus.com/api/swagger/v1/swagger.json", "API V1.0");
    });
}
*/

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Server is running!");

app.Run();
