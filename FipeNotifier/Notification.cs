using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Globalization;
using static FipeNotifier.FipeApiClient;

namespace FipeNotifier
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string BrandCode { get; set; }
        public string ModelCode { get; set; }
        public string YearCode { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public CarPrice Price { get; set; }

        public Notification(Notify notify, Price price)
        {
            BrandCode = notify.BrandCode;
            Email = notify.Email;
            ModelCode = notify.ModelCode;
            YearCode = notify.YearCode;
            Price = price.Valor;
            Description = price.Modelo;
        }

        private Notification() { }

        public Notification ChangePrice(decimal price)
        {
            return new Notification()
            {
                Id = Id,
                BrandCode = BrandCode,
                Description = Description,
                Email = Email,
                Price = price,
                ModelCode = ModelCode,
                YearCode = YearCode
            };
        }

        public void Deconstruct(out string brandCode, out string modelCode, out string yearCode)
        {
            brandCode = BrandCode;
            modelCode = ModelCode;
            yearCode = YearCode;
        }

        public override string ToString()
        => $"Notification ID: {Id} Email: {Email} Price: {Price} " +
            $"Brand/Model/Year: {BrandCode}/{ModelCode}/{YearCode}";
        
        public class CarPrice : IEquatable<decimal>
        {
            public decimal Value { get; set; }

            public CarPrice(decimal value)
                => Value = value;

            public bool Equals(decimal other)
                => Value == other;

            public override bool Equals(object obj)
                => Equals((decimal)obj);

            public static implicit operator CarPrice(decimal value) 
                => new(value);

            public override int GetHashCode()
                => Value.GetHashCode();

            public override string ToString()
                => $"R$ {Value.ToString(new CultureInfo("pt-BR"))}";
        }
    }
}
