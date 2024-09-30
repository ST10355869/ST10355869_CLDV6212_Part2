using Microsoft.AspNetCore.Mvc;
using SemesterTwo.Models;
using SemesterTwo.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace SemesterTwo.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableService _tableService;
        private readonly FileService _fileService;
        private readonly HttpClient _httpClient;
        private readonly string _storeOrderProcessUrl;
        private readonly QueueService _queueService;

        public HomeController(BlobService blobService, TableService tableService, QueueService queueService, FileService fileService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _blobService = blobService;
            _tableService = tableService;
            _httpClient = httpClientFactory.CreateClient();
            _fileService = fileService;
            _queueService = queueService;
            _storeOrderProcessUrl = configuration["AzureFunctions:StoreOrderProcessURL"];
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
            if (string.IsNullOrEmpty(orderId))
            {
                return BadRequest("Order ID cannot be null or empty.");
            }

            try
            {
                await _queueService.SendMessageAsync($"Processing order {orderId}");
                return RedirectToAction("Index");
            }
            catch (HttpRequestException ex)
            {
                // Handle the error appropriately, e.g., log the error or show a user-friendly message
                return StatusCode(500, $"Failed to process the order: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadContract(IFormFile file)
        {
            if (file != null)
            {
                using var stream = file.OpenReadStream();
                await _fileService.UploadFileAsync("contracts-logs", file.FileName, stream);
            }
            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
