using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using PokeNet.Application.Services;
using PokeNet.Application.Settings;
using PokeNet.Application.Swagger;
using PokeNet.Application.UseCase;
using PokeNet.Application.UseCases;
using PokeNet.Application.Validators;
using PokeNet.Infrastructure.Context;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ────────────────────────────────────────────
// API VERSIONING
// ────────────────────────────────────────────
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// ────────────────────────────────────────────
// SWAGGER VERSIONADO
// ────────────────────────────────────────────
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite: Bearer {seu token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});


builder.Services.AddEndpointsApiExplorer();

// ────────────────────────────────────────────
// JWT AUTHENTICATION
// ────────────────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var key = builder.Configuration["Jwt:Key"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

// ────────────────────────────────────────────
// ApiPokemon
// ────────────────────────────────────────────

// ApiPokemon
builder.Services.AddHttpClient<PokemonApiService>(client =>
{
    client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
});

builder.Services.AddHttpClient<ItemUseCase>(client =>
{
    client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
});

builder.Services.AddHttpClient<NatureUseCase>(client =>
{
    client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
});

builder.Services.AddHttpClient<RegionUseCase>(client =>
{
    client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
});

// ────────────────────────────────────────────
// CONTROLLERS
// ────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ────────────────────────────────────────────
// VALIDATION
// ────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RequestUsuarioValidator>();

// ────────────────────────────────────────────
// REPOSITORIES
// ────────────────────────────────────────────
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ────────────────────────────────────────────
// USE CASES
// ────────────────────────────────────────────
builder.Services.AddScoped<AuthUseCase>();
builder.Services.AddScoped<PokemonUseCase>();
builder.Services.AddScoped<UsuarioUseCase>();


// ────────────────────────────────────────────
// MONGO SETTINGS & CONTEXT
// ────────────────────────────────────────────
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoSettings");
    return new MongoClient(settings["ConnectionString"]);
});

builder.Services.AddSingleton<MongoDbContext>();

// ────────────────────────────────────────────
// HEALTHCHECKS
// ────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddMongoDb(
        builder.Configuration["MongoSettings:ConnectionString"],
        name: "MongoDB",
        timeout: TimeSpan.FromSeconds(5)
    );

builder.Services.AddHealthChecksUI(opt =>
{
    opt.SetEvaluationTimeInSeconds(10);
    opt.MaximumHistoryEntriesPerEndpoint(60);
    opt.AddHealthCheckEndpoint("API Health", "/health");
})
.AddInMemoryStorage();


// ────────────────────────────────────────────
// CORS
// ────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
            policy
                .AllowAnyOrigin()   // ou .WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});





// ────────────────────────────────────────────
// BUILD
// ────────────────────────────────────────────
var app = builder.Build();


app.UseCors("AllowFrontend");


// ────────────────────────────────────────────
// SWAGGER UI
// ────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{desc.GroupName}/swagger.json",
                $"PokeNet API {desc.GroupName.ToUpper()}"
            );
        }

        options.RoutePrefix = "swagger";
    });
}

// ────────────────────────────────────────────
// PIPELINE
// ────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});

app.MapControllers();

app.Run();
