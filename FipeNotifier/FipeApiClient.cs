using Newtonsoft.Json;
using static FipeNotifier.FipeApiClient;

namespace FipeNotifier
{
    public interface IFipeApiClient
    {
        public Task<IEnumerable<Brand>> GetBrands();
        public Task<IEnumerable<Model>> GetModelsBy(string brandCode);
        public Task<IEnumerable<Year>> GetYearsBy(string brandCode, string modelCode);
        public Task<CarValue> GetCarValueBy(string brandCode, string modelCode, string yearCode);
    }

    public class FipeApiClient : IFipeApiClient
    {
        private readonly HttpClient _client;

        public FipeApiClient(IHttpClientFactory clientFactory)//adicionar options 
            => _client = clientFactory.CreateClient();

        public async Task<IEnumerable<Brand>> GetBrands()
        {
            var response = await _client.GetAsync("https://parallelum.com.br/fipe/api/v1/carros/marcas");
            var content = await response.Content.ReadAsStringAsync();

            if (content is not null && response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<IEnumerable<Brand>>(content);

            return Enumerable.Empty<Brand>();
        }

        public async Task<IEnumerable<Model>> GetModelsBy(string brandCode)
        {
            var response = await _client.GetAsync($"https://parallelum.com.br/fipe/api/v1/carros/marcas/{brandCode}/modelos");
            var content = await response.Content.ReadAsStringAsync();

            var models = new ModelsResponse(Enumerable.Empty<Model>());

            if (content is not null && response.IsSuccessStatusCode)
                models = JsonConvert.DeserializeObject<ModelsResponse>(content);

            return models.Modelos;
        }

        public async Task<IEnumerable<Year>> GetYearsBy(string brandCode, string modelCode)
        {
            var response = await _client.GetAsync($"https://parallelum.com.br/fipe/api/v1/carros/marcas/{brandCode}/modelos/{modelCode}/anos");
            var content = await response.Content.ReadAsStringAsync();

            if (content is not null && response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<IEnumerable<Year>>(content);
                    
            return Enumerable.Empty<Year>();
        }

        public async Task<CarValue> GetCarValueBy(string brandCode, string modelCode, string yearCode)
        {
            var response = await _client.GetAsync($"https://parallelum.com.br/fipe/api/v1/carros/marcas/{brandCode}/modelos/{modelCode}/anos/{yearCode}");
            var content = await response.Content.ReadAsStringAsync();

            if (content is not null && response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<CarValue>(content);

            return new CarValue(string.Empty, string.Empty, string.Empty);
        }

        public record Brand(string Nome, string Codigo);
        public record ModelsResponse(IEnumerable<Model> Modelos);
        public record Model(string Nome, string Codigo);
        public record Year(string Nome, string Codigo);
        public record CarValue(string Valor, string Marca, string Modelo);
    }
}
