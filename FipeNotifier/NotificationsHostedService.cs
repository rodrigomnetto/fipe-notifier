using FipeNotifier.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FipeNotifier
{
    public class NotificationsHostedService : BackgroundService
    {
        private readonly INotificationService _notificationService;
        private readonly IFipeApiClient _fipeApiClient;
        private readonly ILogger<NotificationsHostedService> _logger;
        private readonly EmailServiceSettings _emailServiceSettings;

        public NotificationsHostedService(INotificationService notificationService
            , IFipeApiClient fipeApiClient
            , IOptions<EmailServiceSettings> emailServiceSettings
            , ILogger<NotificationsHostedService> logger)
        {
            _notificationService = notificationService;
            _fipeApiClient = fipeApiClient;
            _emailServiceSettings = emailServiceSettings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested is false)
            {
                var notifications = await _notificationService.GetAllNotifications();

                using var smtpClient = new SmtpClient();
                smtpClient.Connect(_emailServiceSettings.Host, _emailServiceSettings.Port, true, stoppingToken);
                smtpClient.Authenticate(_emailServiceSettings.Username, _emailServiceSettings.Password, stoppingToken);

                foreach (var notification in notifications)
                {
                    try
                    {
                        var (brandCode, modelCode, yearCode) = notification;
                        var price = await _fipeApiClient.GetPriceBy(brandCode, modelCode, yearCode);

                        if (notification.Price.Equals(price) is false)
                        {
                            var newNotification = notification.ChangePrice(price);
                            var message = GetMessage(notification, newNotification);
                            await smtpClient.SendAsync(message);
                            await _notificationService.UpdateNotification(newNotification);
                        }
                    } 
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Ocorreu um erro ao verificar uma notificação. {notification}");
                    }
                }

                smtpClient.Disconnect(true, stoppingToken);
                await Task.Delay(600000, stoppingToken);
            }
        }

        private static MimeMessage GetMessage(Notification oldNotification, Notification newNotification)
        {
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("Fipe Notifier", "rodrigo.netto.developer@gmail.com"));
            mailMessage.To.Add(new MailboxAddress(string.Empty, newNotification.Email));
            mailMessage.Subject = "Atualização de preço FIPE";
            mailMessage.Body = new TextPart("plain")
            {
                Text = $"Olá, \n O veículo {newNotification.Description} sofreu uma alteração de valor na tabela FIPE. " +
                    $"\n Antigo valor: {oldNotification.Price}, Novo valor: {newNotification.Price}",
            };

            return mailMessage;
        }
    }
}
