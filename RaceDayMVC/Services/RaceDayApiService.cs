
namespace RaceDayMVC.Services
{
    public class RaceDayApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RaceDayApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public HttpClient CreateClient()
        {
            return _httpClientFactory.CreateClient("RaceDayAPI");
        }
    }
}

