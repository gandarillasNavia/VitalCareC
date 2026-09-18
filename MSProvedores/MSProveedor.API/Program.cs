using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MSProveedor.Dominio.Interfaces;
using MSProveedor.Infraestructura.Repositorios;
using MSProveedor.Aplicacion.InputPorts;
using MSProveedor.Aplicacion.Interactors;

Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);

// 1. Habilitar la carpeta Controllers
builder.Services.AddControllers(); 
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// =======================================================
// 2. INYECCIÓN DE DEPENDENCIAS (LA MAGIA DE LA ARQUITECTURA)
// =======================================================
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IProveedorInputPort, ProveedorInteractor>();

// Configuración JWT
string jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new InvalidOperationException("No se encontro JWT_KEY en variables de entorno.");
string jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? throw new InvalidOperationException("No se encontro JWT_ISSUER en variables de entorno.");
string jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? throw new InvalidOperationException("No se encontro JWT_AUDIENCE en variables de entorno.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? "http://localhost:5081")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// 3. Mapear los controladores
app.MapControllers(); 

await app.RunAsync();