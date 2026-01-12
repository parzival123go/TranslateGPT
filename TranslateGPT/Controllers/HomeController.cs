using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TranslateGPT.Models;

namespace TranslateGPT.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly List<string> mostUsedLanguages = new List<string>
        {
            "English",
            "Spanish",
            "French",
            "German",
            "Chinese",
            "Japanese",
            "Russian",
            "Portuguese",
            "Italian",
            "Arabic",
            "Hindi",
            "Turkish",
            "Korean",
            "Dutch",
            "Swedish",
            "Polish",
            "Vietnamese",
            "Thai",
            "Indonesian",
            "Greek",
            "Czech",
            "Hungarian",
            "Finnish",
            "Danish",
            "Norwegian",
            "Hebrew",
            "Ukrainian",
            "Romanian",
            "Bulgarian",
            "Slovak",
            "Croatian",
            "Serbian",
            "Lithuanian",
            "Latvian",
            "Estonian",
            "Filipino",
            "Malay"
        };
        public IActionResult Index()
        {
            ViewBag.Languages = new SelectList(mostUsedLanguages);
            return View();
        }

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> OpenAIGPT(string query, string selectedLanguage)
        {
            //Get the OpenAI API key from environment variables
            var openAiApiKey = _configuration["OpenAI:APIKey"];

            //Set up the HTTP client with OpenAI API key
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

            //Define the request payload
            var payload = new
            {
                model = "o4-mini",
                messages = new object[]
                {
                    new { role = "system", content = $"Translate to {selectedLanguage}"},
                    new { role = "user", content = query }
                },
                max_tokens = 256,
                temperature = 0
            };

            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            HttpContent content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {openAiApiKey}");
            request.Content = content;

            var responseMessage = await _httpClient.SendAsync(request);
            var responseMessageJson = await responseMessage.Content.ReadAsStringAsync();

            ViewBag.Languages = new SelectList(mostUsedLanguages);

            if (responseMessage.IsSuccessStatusCode)
            {
                var response = System.Text.Json.JsonSerializer.Deserialize<DTOs.OpenAIResponse>(responseMessageJson);
                ViewBag.Result = response?.Choices?[0]?.Message?.Content;
            }
            else
            {
                // Log or display the specific error message (like "insufficient_quota")
                ViewBag.Error = "OpenAI API Error: Please check your billing/quota status.";
                _logger.LogError($"OpenAI API Failure: {responseMessageJson}");
            }

            return View("Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
