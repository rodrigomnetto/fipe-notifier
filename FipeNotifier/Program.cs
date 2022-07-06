using FipeNotifier;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<FipeNotifierDatabaseSettings>(
    builder.Configuration.GetSection("FipeNotifierDatabase"));

builder.Services.AddHttpClient();


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

    return await notificationService.GetNotifications(email);
});

app.MapPost("/notifications", async (Notify notify
    , [FromServices] IFipeApiClient fipeClient
    , [FromServices] INotificationService notificationService) => {

    var carValue = await fipeClient.GetCarValueBy(notify.BrandCode, notify.ModelCode, notify.YearCode);
    return await notificationService.RegisterNotification(notify, carValue);
});

app.MapDelete("/notifications/{id}", async (string id
    , [FromServices] INotificationService notificationService) => {

    return await notificationService.DeleteNotification(id);
});

app.Run();

