using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Mundialito.Api.Extensions;
using Mundialito.Api.Idempotency;
using Mundialito.Api.Middlewares;
using Mundialito.Application;
using Mundialito.Infrastructure;
using Mundialito.Infrastructure.Persistence.Seed;
using Serilog;

// Logger mínimo para registrar errores que ocurran antes de leer la configuración.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Local: User Secrets. Docker: variable de entorno ConnectionStrings__DefaultConnection.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Falta la connection string 'DefaultConnection' (User Secrets en local o variable de entorno en Docker).");
    }

    builder.Services
        .AddInfrastructure(connectionString)
        .AddApplicationHandlers(ApplicationAssembly.Instance);

    builder.Services
        .AddControllers(options =>
        {
            // Los campos obligatorios los valida el dominio (con mensajes en español), no MVC.
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        })
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Mundialito API",
            Version = "v1",
            Description = "Gestión del Mundialito de Fútbol Corporativo: equipos, jugadores, partidos, tabla y goleadores."
        });
        options.OperationFilter<IdempotencyKeyHeaderOperationFilter>();
    });

    const string FrontendCorsPolicy = "Frontend";
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders(
            CorrelationIdMiddleware.CorrelationIdHeader,
            CorrelationIdMiddleware.TraceIdHeader,
            IdempotencyFilter.ReplayedHeaderName)));

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseMiddleware<CorrelationIdMiddleware>();

    // Un log por request con su duración, enlazado al TraceId y al CorrelationId.
    app.UseSerilogRequestLogging(options =>
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms");

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(FrontendCorsPolicy);
    app.MapControllers();

    await app.Services.InitializeDatabaseAsync();

    await app.RunAsync();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "La API se detuvo por un error inesperado");
}
finally
{
    await Log.CloseAndFlushAsync();
}
