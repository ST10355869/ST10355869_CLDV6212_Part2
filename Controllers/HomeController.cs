using Microsoft.AspNetCore.Mvc;
using SemesterTwo.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SemesterTwo.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HomeController> _logger;


        public HomeController(HttpClient httpClient, ILogger<HomeController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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

            var containerName = "product-images";
            var blobName = $"{productName}.jpg";

            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

            var url = $"https://st10355869functionapp.azurewebsites.net/api/UploadBlob?{containerName}&blobName={blobName}";
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                ViewBag.Message = result;
                return RedirectToAction("Details", new { name = productName });
            }

            ViewBag.Error = "Failed to upload image.";
            _logger.LogError("Failed to upload image.");
            return View("Error");
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

            var url = $"https://st10355869functionapp.azurewebsites.net/api/StoreTableInfo?";
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
            var url = $"https://st10355869functionapp.azurewebsites.net/api/ProcessQueueMessage?";
            var message = new { queueName = "order-processing", message = $"Processing order {orderId}" };

            var response = await _httpClient.PostAsJsonAsync(url, message);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Failed to process order.";
            _logger.LogError("Failed to process order.");
            return View("Error");
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

            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

            var url = $"https://st10355869functionapp.azurewebsites.net/api/UploadFile?{fileShareName}&fileName={fileName}";
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