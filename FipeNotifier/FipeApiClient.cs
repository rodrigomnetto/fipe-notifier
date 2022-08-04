using FipeNotifier.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;
using static FipeNotifier.FipeApiClient;

namespace FipeNotifier
{
    public interface IFipeApiClient
    {
        public Task<IEnumerable<Brand>> GetBrands();
        public Task<IEnumerable<Model>> GetModelsBy(string brandCode);
        public Task<IEnumerable<Year>> GetYearsBy(string brandCode, string modelCode);
        public Task<Price> GetPriceBy(string brandCode, string modelCode, string yearCode);
    }

    public class FipeApiClient : IFipeApiClient
    {
        private readonly HttpClient _client;
        private readonly FipeClientSettings _fipeSettings;

        public FipeApiClient(HttpClient httpClient, IOptions<FipeClientSettings> fipeSettings)
        {
            _fipeSettings = fipeSettings.Value;
            _client = httpClient;
            httpClient.BaseAddress = new Uri(_fipeSettings.BaseAddress);
        }

        public async Task<IEnumerable<Brand>> GetBrands()
        {
            var response = await _client.GetAsync(_fipeSettings.GetBrands());
            var content = await response.Content.ReadAsStringAsync();

            if (content is not null && response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<IEnumerable<Brand>>(content);

            return Enumerable.Empty<Brand>();
        }

        public async Task<IEnumerable<Model>> GetModelsBy(string brandCode)
        {
            var response = await _client.GetAsync(_fipeSettings.GetModels(brandCode));
            var content = await response.Content.ReadAsStringAsync();

            var models = new ModelsResponse(Enumerable.Empty<Model>());

            if (content is not null && response.IsSuccessStatusCode)
                models = JsonConvert.DeserializeObject<ModelsResponse>(content);

            return models.Modelos;
        }

        public async Task<IEnumerable<Year>> GetYearsBy(string brandCode, string modelCode)
        {
            var response = await _client.GetAsync(_fipeSettings.GetYears(brandCode, modelCode));
            var content = await response.Content.ReadAsStringAsync();

            if (content is not null && response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<IEnumerable<Year>>(content);
                    
            return Enumerable.Empty<Year>();
        }

        public async Task<Price> GetPriceBy(string brandCode, string modelCode, string yearCode)
        {
            var response = await _client.GetAsync(_fipeSettings.GetPrice(brandCode, modelCode, yearCode));
            var content = await response.Content.ReadAsStringAsync();

            if (content is not null && response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Price>(content);
            
            return new Price(string.Empty, string.Empty, string.Empty);
        }

        public record Brand(string Nome, string Codigo);
        public record ModelsResponse(IEnumerable<Model> Modelos);
        public record Model(string Nome, string Codigo);
        public record Year(string Nome, string Codigo);
        public record Price
        {
            public Price(string valor, string marca, string modelo)
            {
                Valor = Convert.ToDecimal(valor.Replace("R$ ", string.Empty), new CultureInfo("pt-BR"));
                Marca = marca;
                Modelo = modelo;
            }

            public decimal Valor { get; set; }

            public string Marca { get; set; }

            public string Modelo { get; set; }

            public static implicit operator decimal(Price price) => price.Valor;
        };
    }
}
