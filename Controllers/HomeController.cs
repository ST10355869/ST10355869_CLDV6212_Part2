using Microsoft.AspNetCore.Mvc;
using SemesterTwo.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;
using System.Net.Http.Headers;
using System.Web;

namespace SemesterTwo.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;


        public HomeController(HttpClient httpClient, ILogger<HomeController> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Azure Function to upload a product image to Blob Storage
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file, string productName)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return BadRequest("No file uploaded.");
            }

            try
            {
                var containerName = "product-images";
                var blobName = $"{productName}-{Guid.NewGuid()}.jpg";
                var url = $"{_configuration["AzureFunctions:BlobStorage"]}&containerName={containerName}&blobName={blobName}";

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.FileName);

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Image upload successful: {result}");
                    ViewBag.Message = result;
                    return RedirectToAction("Index");  // Or wherever you want to redirect after successful upload
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to upload image. Status: {response.StatusCode}, Error: {errorContent}");
                    ViewBag.Error = $"Failed to upload image: {errorContent}";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading image");
                ViewBag.Error = "An unexpected error occurred while uploading the image.";
                return View("Error");
            }
        }


        // Azure Function to add a customer profile to Table Storage
        [HttpPost]
        public async Task<IActionResult> AddCustomerProfile(CustomerProfile profile)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid profile data.");
                return BadRequest("Invalid profile data.");
            }

            var url = _configuration["AzureFunctions:TableStorage"];
            var response = await _httpClient.PostAsJsonAsync(url, profile);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Failed to add customer profile.";
            _logger.LogError("Failed to add customer profile.");
            return View("Error");
        }


        // Azure Function to send a message to Queue Storage

        [HttpPost]
        public async Task<IActionResult> ProcessOrder(string orderId)
        {
            try
            {
                var url = _configuration["AzureFunctions:StoreOrderProcessURL"];
                var queueName = "order-processing";
                var message = $"Processing order {orderId}";
                var fullUrl = $"{url}?queueName={HttpUtility.UrlEncode(queueName)}&message={HttpUtility.UrlEncode(message)}";

                var response = await _httpClient.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Message added to queue: {result}");
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to add message to queue. Status: {response.StatusCode}, Error: {errorContent}");
                    ViewBag.Error = $"Failed to add message to queue: {errorContent}";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding message to queue");
                ViewBag.Error = "An unexpected error occurred while adding message to queue.";
                return View("Error");
            }
        }

        // Azure Function to upload a file to Azure File Share
        [HttpPost]
        public async Task<IActionResult> UploadContract(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return BadRequest("No file uploaded.");
            }

            var fileShareName = "contracts-logs";
            var fileName = file.FileName;
            var url = $"{_configuration["AzureFunctions:UploadFileURL"]}&fileShareName={fileShareName}&fileName={fileName}";

            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Failed to upload contract.";
            _logger.LogError("Failed to upload contract.");
            return View("Error");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            _logger.LogError("An error occurred.");
            return View();
        }

    }
}