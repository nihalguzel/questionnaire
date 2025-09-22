using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Get()
        {
            string[] lines = {
                "44BB190BC2493964E053CF0A000AB546\t6164\t1\t09/08/2016 09:16:00",
                "44BB190BC24A3964E053CF0A000AB546\t544\t3\t10/08/2016 13:54:00",
                "44BB190BC24B3964E053CF0A000AB546\t9648\t3\t08/08/2016 06:08:00",
                "44BB190BC24C3964E053CF0A000AB546\t7565\t2\t10/08/2016 17:30:00",
                "44BB190BC24D3964E053CF0A000AB546\t8995\t1\t11/08/2016 02:40:00",
                "44BB190BC24E3964E053CF0A000AB546\t4407\t1\t08/08/2016 07:30:00",
                "44BB190BC24F3964E053CF0A000AB546\t5839\t2\t10/08/2016 02:40:00",
                "44BB190BC2503964E053CF0A000AB546\t548\t3\t09/08/2016 20:45:00",
                "44BB190BC2513964E053CF0A000AB546\t376\t3\t10/08/2016 04:57:00",
                "44BB190BC2523964E053CF0A000AB546\t3403\t2\t08/08/2016 21:14:00",
                "44BB190BC2533964E053CF0A000AB546\t7256\t2\t10/08/2016 06:29:00",
                "44BB190BC2543964E053CF0A000AB546\t4291\t3\t08/08/2016 09:26:00",
                "44BB190BC2553964E053CF0A000AB546\t5722\t2\t08/08/2016 23:33:00",
                "44BB190BC2563964E053CF0A000AB546\t9857\t1\t10/08/2016 22:05:00",
                "44BB190BC2573964E053CF0A000AB546\t3122\t2\t09/08/2016 08:35:00",
                "44BB190BC2583964E053CF0A000AB546\t217\t2\t10/08/2016 13:20:00",
                "44BB190BC2593964E053CF0A000AB546\t3022\t1\t10/08/2016 17:06:00",
                "44BB190BC25A3964E053CF0A000AB546\t9857\t1\t10/08/2016 15:06:00",
                "44BB190BC25B3964E053CF0A000AB546\t2168\t3\t11/08/2016 13:30:33"
            };

            var userSongs = new Dictionary<string, HashSet<string>>();

            foreach (var line in lines.Skip(1))
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
