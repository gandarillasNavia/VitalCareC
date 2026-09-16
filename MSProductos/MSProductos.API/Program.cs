using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MSProductos.Aplicacion.InputPorts;
using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Interfaces;
using MSProductos.Dominio.Validadores;
using MSProductos.Infraestructura.Repositorios;
using MSProductos.Infraestructura.Mensajeria;
using MSProductos.Aplicacion.UseCases;

Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IEventPublisher, RabbitPublisher>();
builder.Services.AddHostedService<RabbitConsumerForProductos>();


// Dependencias

builder.Services.AddScoped<IClasificacionRepository, ClasificacionRepository>();

builder.Services.AddScoped<IClasificacionInputPort, ClasificacionInteractor>();

builder.Services.AddScoped<IResult<Clasificacion>, ClasificacionValidador>();

builder.Services.AddScoped<IMedicamentoRepository, MedicamentoRepository>();

builder.Services.AddScoped<IMedicamentoInputPort, MedicamentoInteractor>();

builder.Services.AddScoped<IResult<Medicamento>, MedicamentoValidator>();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();