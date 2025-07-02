using ILR_VALIDATION.Domain.Interfaces;
//using ILR_VALIDATION.Infrastructure.BackgroundServices;
using ILR_VALIDATION.Infrastructure.Services;
using Microsoft.ApplicationInsights.Extensibility; 
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ILR_VALIDATION.Application.Commands.UploadFileCommand).Assembly));
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<IFileStorageService, AzureBlobStorageService>();
builder.Services.AddSingleton<AzureBlobStorageService>();
builder.Services.AddSingleton<IMessageQueueService, AzureServiceBusQueueService>();
//builder.Services.AddHostedService<ResultGeneratorService>();

builder.Services.AddLogging(logging =>
{
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddConsole();
    logging.AddDebug();
    logging.AddApplicationInsights();
});

builder.Services.AddApplicationInsightsTelemetry(); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception?.Message);
        await context.Response.WriteAsync("An error occurred. Check logs for details.");
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();