using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SemesterTwo.Services
{
    public class QueueService
    {
        private readonly HttpClient _httpClient;
        private readonly string _storeOrderProcessUrl;

        public QueueService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClient = new HttpClient();
            _storeOrderProcessUrl = configuration.GetConnectionString("AzureFunctions:StoreOrderProcessURL");
        }

        public async Task SendMessageAsync(string message)
        {
            var requestUrl = $"{_storeOrderProcessUrl}&message={message}";
            var response = await _httpClient.PostAsync(requestUrl, null);

            if (!response.IsSuccessStatusCode)
            {
                // Handle the error response as needed
                throw new HttpRequestException($"Failed to send message: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
    }
}
