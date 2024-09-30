using Microsoft.AspNetCore.Mvc;
using SemesterTwo.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace SemesterTwo.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
                return BadRequest("No file uploaded.");
            }

            var containerName = "product-images";
            var blobName = $"{productName}.jpg";

            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

            var url = $"https://st10355869functionapp.azurewebsites.net/api/UploadBlob?code=07MVeQzF8knvul-DjBrFWCXV7gtA1NlJPY3cbgJPVVAzAzFuvpoz4w%3D%3D{containerName}&blobName={blobName}";
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                ViewBag.Message = result;
                return RedirectToAction("Details", new { name = productName });
            }

            ViewBag.Error = "Failed to upload image.";
            return View("Error");
        }

        // Azure Function to add a customer profile to Table Storage
        [HttpPost]
        public async Task<IActionResult> AddCustomerProfile(CustomerProfile profile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid profile data.");
            }

            var url = $"https://st10355869functionapp.azurewebsites.net/api/StoreTableInfo?code=bJ5RjjvVMyLolpdnMG9aafLM9bIDXJgT7d7ZmZVr-GKQAzFu_clYxA%3D%3D";
            var response = await _httpClient.PostAsJsonAsync(url, profile);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Failed to add customer profile.";
            return View("Error");
        }

        // Azure Function to send a message to Queue Storage
        [HttpPost]
        public async Task<IActionResult> ProcessOrder(string orderId)
        {
            var url = $"https://st10355869functionapp.azurewebsites.net/api/ProcessQueueMessage?code=PcoersGKOlEtRnsFzonQOKJUYz-w3dQwA7JPFxuDx__0AzFu4mt34w%3D%3D";
            var message = new { queueName = "order-processing", message = $"Processing order {orderId}" };

            var response = await _httpClient.PostAsJsonAsync(url, message);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Failed to process order.";
            return View("Error");
        }


        // Azure Function to upload a file to Azure File Share
        [HttpPost]
        public async Task<IActionResult> UploadContract(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var fileShareName = "contracts-logs";
            var fileName = file.FileName;

            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

            var url = $"https://st10355869functionapp.azurewebsites.net/api/UploadFile?code=6Yri4seSo50y9XVCtfRrnse1CYa7fEnFwjbtARpX8O-aAzFuHjZFQA%3D%3D{fileShareName}&fileName={fileName}";
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Failed to upload contract.";
            return View("Error");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}