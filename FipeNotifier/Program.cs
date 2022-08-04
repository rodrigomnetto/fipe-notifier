using FipeNotifier;
using FipeNotifier.Settings;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.File("logs/log.txt")
    .MinimumLevel.Is(LogEventLevel.Warning));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<FipeNotifierDatabaseSettings>(
    builder.Configuration.GetSection("FipeNotifierDatabase"));
builder.Services.Configure<EmailServiceSettings>(
    builder.Configuration.GetSection("EmailService"));
builder.Services.Configure<FipeClientSettings>(
    builder.Configuration.GetSection("FipeClient"));

builder.Services.AddHostedService<NotificationsHostedService>();

builder.Services.AddHttpClient<FipeApiClient>();

builder.Services.AddScoped<IFipeApiClient, FipeApiClient>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/brands", async ([FromServices] IFipeApiClient fipeClient) => {
    return await fipeClient.GetBrands();
});

app.MapGet("/brands/{brandCode}/models", async (string brandCode
    , [FromServices] IFipeApiClient fipeClient) => {

    return await fipeClient.GetModelsBy(brandCode);
});

app.MapGet("/brands/{brandCode}/models/{modelCode}/years", async (string brandCode
    , string modelCode
    , [FromServices] IFipeApiClient fipeClient) => {

    return await fipeClient.GetYearsBy(brandCode, modelCode);
});

app.MapGet("/notifications", async (string email
    , [FromServices] INotificationService notificationService) => {

    var result = await notificationService.GetNotifications(email);
    return result.Any() ? Results.Ok(result) : Results.NoContent();
    });

app.MapPost("/notifications", async (Notify notify
    , [FromServices] IFipeApiClient fipeClient
    , [FromServices] INotificationService notificationService) => {

    var price = await fipeClient.GetPriceBy(notify.BrandCode, notify.ModelCode, notify.YearCode);
    return await notificationService.RegisterNotification(notify, price);
});

app.MapDelete("/notifications/{id}",
    async (string id, [FromServices] INotificationService notificationService) => {

        var result = await notificationService.DeleteNotification(id);
        return result ? Results.Ok() : Results.NoContent();
    }
);

app.Run();

