using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Globalization;
using static FipeNotifier.FipeApiClient;

namespace FipeNotifier
{
    public interface INotificationService
    {
        Task<Notification> RegisterNotification(Notify notify, CarValue carValue);//trocar nome carvalue pra outro

        Task<bool> DeleteNotification(string id);

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
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<Notification>> GetNotifications(string email)
        {
            var result = await _notificationsCollection.FindAsync(x => x.Email == email);
            return result.ToEnumerable();
        }

        public async Task<Notification> RegisterNotification(Notify notify, CarValue carValue)
        {
            var existingNotifications = await GetNotifications(notify.Email);

            var found = existingNotifications.Where(x => x.BrandCode == notify.BrandCode
                 && x.ModelCode == notify.ModelCode
                 && x.YearCode == notify.YearCode);

            if (found.Any()) return found.First();

            var notification = new Notification(notify, carValue);
            await _notificationsCollection.InsertOneAsync(notification);

            return notification; //ajustar pra retornar objeto salvo com id
        }

        public async Task<IEnumerable<Notification>> GetAllNotifications()
        {
            var result = await _notificationsCollection.FindAsync(_ => true);
            return result.ToEnumerable();
        }
    }

    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string BrandCode { get; set; }
        public string ModelCode { get; set; }
        public string YearCode { get; set; }
        public string Email { get; set; }
        public DecimalValue Value { get; set; }

        public Notification(Notify notify, CarValue carValue)
        {
            BrandCode = notify.BrandCode;
            Email = notify.Email;
            ModelCode = notify.ModelCode;
            YearCode = notify.YearCode;
            Value = carValue.Valor;
        }

        public void Deconstruct(out string brandCode, out string modelCode, out string yearCode)
        {
            brandCode = BrandCode;
            modelCode = ModelCode;
            yearCode = YearCode;
        }

        public class DecimalValue : IEquatable<string> //trocar nome e mover tudo pra outro arquivo
        {
            public decimal Value { get; set; }

            public DecimalValue(string value) 
                => Value = ToDecimal(value);

            private static decimal ToDecimal(string value) 
                => Convert.ToDecimal(value.Replace("R$ ", string.Empty), new CultureInfo("pt-BR"));

            public bool Equals(string other)
                => other is not null && Value == ToDecimal(other);

            public override bool Equals(object obj)
                => Equals((string)obj);

            public override int GetHashCode()
                => Value.GetHashCode();

            public static implicit operator DecimalValue(string value) 
                => new(value);
        }
    }

    public record Notify(string BrandCode, string ModelCode, string YearCode, string Email);
}