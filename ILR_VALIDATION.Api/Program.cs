using ILR_VALIDATION.Infrastructure.BackgroundServices;
using ILR_VALIDATION.Infrastructure.Services;
using ILR_VALIDATION.Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
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
builder.Services.AddSingleton<IMessageQueueService, AzureServiceBusQueueService>();
builder.Services.AddHostedService<ResultGeneratorService>();

builder.Services.AddLogging(logging =>
{
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();