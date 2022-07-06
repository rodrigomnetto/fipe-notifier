namespace FipeNotifier
{
    public class NotificationsHostedService : BackgroundService
    {
        public NotificationsHostedService(INotificationService notificationService, IFipeApiClient fipeApiClient)
        {
            _notificationService = notificationService;
            _fipeApiClient = fipeApiClient;
        }

        private readonly INotificationService _notificationService;
        private readonly IFipeApiClient _fipeApiClient;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested is false)
            {
                var notifications = await _notificationService.GetAllNotifications();

                foreach (var notification in notifications)
                {
                    var (brandCode, modelCode, yearCode) = notification;

                    var carValue = await _fipeApiClient.GetCarValueBy(brandCode, modelCode, yearCode);

                    if (notification.Value.Equals(carValue.Valor) is false)
                    {
                        //notificar


                        //atualizar
                    }
                }

                await Task.Delay(600000, stoppingToken);
            }
        }
    }
}
