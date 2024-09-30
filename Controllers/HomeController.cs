using Microsoft.AspNetCore.Mvc;
using SemesterTwo.Models;
using SemesterTwo.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SemesterTwo.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableService _tableService;
        private readonly QueueService _queueService;
        private readonly FileService _fileService;
        private readonly HttpClient _httpClient;

        public HomeController(BlobService blobService, TableService tableService, QueueService queueService, FileService fileService, IHttpClientFactory httpClient)
        {
            _blobService = blobService;
            _tableService = tableService;
            _queueService = queueService;
            _fileService = fileService;
            _httpClient = httpClientFactory.CreateClient();

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file != null)
            {
                using var stream = file.OpenReadStream();
                await _blobService.UploadBlobAsync("product-images", file.FileName, stream);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomerProfile(CustomerProfile profile)
        {
            if (ModelState.IsValid)
            {
                await _tableService.AddEntityAsync(profile);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ProcessOrder(string orderId)
        {
            await _queueService.SendMessageAsync("order-processing", $"Processing order {orderId}");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadContract(IFormFile file)
        {
            if (file != null)
            {
                string shareName = "contracts-logs"; // Azure Files share name
                string fileName = file.FileName; // The name of the file being uploaded

                // The URL for the Azure Function
                string functionUrl = $"https://st10355869functionapp.azurewebsites.net/api/UploadFile?code=6Yri4seSo50y9XVCtfRrnse1CYa7fEnFwjbtARpX8O-aAzFuHjZFQA%3D%3D&shareName={shareName}&fileName={fileName}";

                // Open a stream to read the file
                using var stream = file.OpenReadStream();
                using var content = new StreamContent(stream);

                // Set the content type
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Make the HTTP POST request to the Azure Function
                HttpResponseMessage response = await _httpClient.PostAsync(functionUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Handle success - e.g., show a success message
                    TempData["Message"] = "File uploaded successfully.";
                }
                else
                {
                    // Handle failure - e.g., log error or show an error message
                    TempData["Message"] = "File upload failed.";
                }
            }

            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
