using FipeNotifier.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using static FipeNotifier.FipeApiClient;

namespace FipeNotifier
{
    public interface INotificationService
    {
        Task<Notification> RegisterNotification(Notify notify, Price price);

        Task<bool> DeleteNotification(string id);

        Task<bool> UpdateNotification(Notification notification);

        Task<IEnumerable<Notification>> GetNotifications(string email);

        Task<IEnumerable<Notification>> GetAllNotifications();
    }

    public class NotificationService : INotificationService
    {

        private readonly IMongoCollection<Notification> _notificationsCollection;

        public NotificationService(IOptions<FipeNotifierDatabaseSettings> fipeNotifierDatabaseSettings)
        {
            var settings = fipeNotifierDatabaseSettings.Value;
            var mongoClient = new MongoClient(settings.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);

            _notificationsCollection = mongoDatabase.GetCollection<Notification>(
                settings.NotificationsCollectionName);
        }

        public async Task<bool> DeleteNotification(string id)
        {
            var result = await _notificationsCollection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount == 1;
        }

        public async Task<bool> UpdateNotification(Notification notification)
        {
            var filter = Builders<Notification>.Filter.Eq(nameof(notification.Id), notification.Id);
            var update = Builders<Notification>.Update.Set(x => x.Price.Value, notification.Price.Value);

            var result = await _notificationsCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount == 1;
        }

        public async Task<IEnumerable<Notification>> GetNotifications(string email)
        {
            var result = await _notificationsCollection.FindAsync(x => x.Email == email);
            return result.ToList();
        }

        public async Task<Notification> RegisterNotification(Notify notify, Price price)
        {
            var existingNotifications = await GetNotifications(notify.Email);

            var found = existingNotifications.Where(x => x.BrandCode == notify.BrandCode
                 && x.ModelCode == notify.ModelCode
                 && x.YearCode == notify.YearCode);

            if (found.Any()) return found.First();

            var notification = new Notification(notify, price);
            await _notificationsCollection.InsertOneAsync(notification);

            return notification;
        }

        public async Task<IEnumerable<Notification>> GetAllNotifications()
        {
            var result = await _notificationsCollection.FindAsync(_ => true);
            return result.ToList();
        }
    }

    public record Notify(string BrandCode, string ModelCode, string YearCode, string Email);
}