using DataLayer.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using task.Servicies;

namespace task.Repository
{
    public interface IJsonRepository
    {
        Task<JsonDeserialize> LoadJson(string source);
    }

    public class JsonRepository(ILogger<Worker> logger) : IJsonRepository
    {
        private readonly ILogger<Worker> _logger = logger;
        public async Task<JsonDeserialize?> LoadJson(string source)
        {
            try
            {
                JsonDeserialize result;
                var assembly = Assembly.GetExecutingAssembly();

                using (Stream stream = assembly.GetManifestResourceStream(source))
                using (StreamReader reader = new(stream))
                {
                    _logger.LogInformation($"Read and deserialize json file.");
                    string json = await reader.ReadToEndAsync();
                    result = JsonSerializer.Deserialize<JsonDeserialize>(json);
                    _logger.LogInformation($"Deserializing complete. Get {result.city.Count()} record of city.");
                }

                return result;
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
