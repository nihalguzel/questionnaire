using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text;

namespace questionnaire.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BigData : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<BigData> _logger;

        public BigData(ILogger<BigData> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://pi.works/exhibit-a");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Data source unreachable.");

            // ZIP dosyasýný byte olarak al
            var zipBytes = await response.Content.ReadAsByteArrayAsync();

            string[] lines;
            using (var zipStream = new MemoryStream(zipBytes))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                var entry = archive.GetEntry("exhibitA-input.csv");
                if (entry == null)
                    return StatusCode(500, "CSV file not found in archive.");

                using var entryStream = entry.Open();
                using var reader = new StreamReader(entryStream, Encoding.UTF8);
                var csvContent = await reader.ReadToEndAsync();
                lines = csvContent
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToArray();
            }

            var userSongs = new Dictionary<string, HashSet<string>>();

            foreach (var line in lines.Skip(1)) // ilk satýr baþlýk
            {
                var parts = line.Split('\t');
                if (parts.Length < 4) continue;

                string songId = parts[1].Trim();
                string clientId = parts[2].Trim();
                string playTs = parts[3].Trim();

                if (DateTime.TryParse(playTs, out DateTime dt))
                {
                    if (dt.Date == new DateTime(2016, 8, 10))
                    {
                        if (!userSongs.ContainsKey(clientId))
                            userSongs[clientId] = new HashSet<string>();

                        userSongs[clientId].Add(songId);
                    }
                }
            }

            var distribution = userSongs
                .GroupBy(kv => kv.Value.Count)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = distribution
                .OrderBy(kv => kv.Key)
                .Select(kv => new { DISTINCT_PLAY_COUNT = kv.Key, CLIENT_COUNT = kv.Value })
                .ToList();

            return Ok(result);
        }
    }
}
