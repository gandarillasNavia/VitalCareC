using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;
using Microsoft.OpenApi.Models;
using MSUsuarios.App.Interfaces;
using MSUsuarios.App.Servicios;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Dominio.Validadores;
using MSUsuarios.Infraestructura.Creadores;

Env.Load("../../.env");
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MSUsuarios API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT como: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    string[] allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? ["http://localhost:5081"];

    options.AddPolicy("FrontendClients", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configuración JWT
string jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("No se encontro JWT_KEY en variables de entorno ni Jwt:Key en configuracion.");
string jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("No se encontro JWT_ISSUER en variables de entorno ni Jwt:Issuer en configuracion.");
string jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("No se encontro JWT_AUDIENCE en variables de entorno ni Jwt:Audience en configuracion.");
int jwtExpirationMinutes = int.TryParse(
    Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES") 
    ?? builder.Configuration["Jwt:ExpirationMinutes"]?.ToString(), 
    out int expiration) ? expiration : 60;

builder.Services.AddScoped<UsuarioRepositoryCreator>();
builder.Services.AddScoped<UsuarioTokenRepositoryCreator>();

builder.Services.AddScoped<IUsuarioRepository>(provider =>
{
    UsuarioRepositoryCreator creator = provider.GetRequiredService<UsuarioRepositoryCreator>();
    return creator.CreateRepo();
});

builder.Services.AddScoped<IUsuarioTokenRepository>(provider =>
{
    UsuarioTokenRepositoryCreator creator = provider.GetRequiredService<UsuarioTokenRepositoryCreator>();
    return creator.CreateRepo();
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IUsuarioTokenService, UsuarioTokenService>();
builder.Services.AddScoped<UsuarioValidacionGeneral>();
builder.Services.AddScoped<ValidadorContraseña>();
builder.Services.AddScoped<ValidadorCambioContraseña>();
builder.Services.AddScoped<UsuarioValidadores>();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendClients");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();