using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gsmsharing.Models.APIGPT
{
    public class AIContentGenerator
    {
        private readonly HttpClient _client;
        private const string ApiKey = "72206e7661msh5e52039ad555765p1c43d9jsn7442bae15ba5";
        private const string ApiHost = "chat-gpt26.p.rapidapi.com";

        public AIContentGenerator()
        {
            _client = new HttpClient();
        }

        public async Task<AIGeneratedContent> GenerateContentAsync(string title)
        {
            var prompt = $"Generate the following for the title '{title}':\n" +
                         "1. Meta description (150-160 characters)\n" +
                         "2. Meta keywords (comma-separated)\n" +
                         "3. OG title (50-60 characters)\n" +
                         "4. Description (200-250 characters)\n" +
                         "Format the response as JSON with keys: metaDescription, metaKeywords, ogTitle, description";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://chat-gpt26.p.rapidapi.com/"),
                Headers =
                {
                    { "x-rapidapi-key", ApiKey },
                    { "x-rapidapi-host", ApiHost },
                },
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                }))
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                }
            };

            using var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            // Parse the JSON response
            var jsonResponse = JObject.Parse(body);
            var content = jsonResponse["choices"][0]["message"]["content"].ToString();

            // Parse the content as JSON
            try
            {
                return JsonConvert.DeserializeObject<AIGeneratedContent>(content);
            }
            catch (JsonException)
            {
                // If parsing as JSON fails, try to extract the information manually
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var generatedContent = new AIGeneratedContent();

                foreach (var line in lines)
                {
                    if (line.StartsWith("Meta description:"))
                        generatedContent.MetaDescription = line.Substring("Meta description:".Length).Trim();
                    else if (line.StartsWith("Meta keywords:"))
                        generatedContent.MetaKeywords = line.Substring("Meta keywords:".Length).Trim();
                    else if (line.StartsWith("OG title:"))
                        generatedContent.OgTitle = line.Substring("OG title:".Length).Trim();
                    else if (line.StartsWith("Description:"))
                        generatedContent.Description = line.Substring("Description:".Length).Trim();
                }

                return generatedContent;
            }
        }
    }
}