namespace FipeNotifier.Settings
{
    public class FipeClientSettings
    {
        public string BaseAddress { get; set; }

        public string GetBrands()
        => "/fipe/api/v1/carros/marcas";

        public string GetModels(string brandCode)
        => $"/fipe/api/v1/carros/marcas/{brandCode}/modelos";

        public string GetYears(string brandCode, string modelCode)
        => $"/fipe/api/v1/carros/marcas/{brandCode}/modelos/{modelCode}/anos";

        public string GetPrice(string brandCode, string modelCode, string yearCode)
        => $"/fipe/api/v1/carros/marcas/{brandCode}/modelos/{modelCode}/anos/{yearCode}";
    }
}
