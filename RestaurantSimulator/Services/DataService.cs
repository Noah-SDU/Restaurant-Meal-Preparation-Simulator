using System.IO;
using System.Text.Json;
using RestaurantSimulator.Models;


namespace RestaurantSimulator.Services
{
    public interface IRestaurantDataService
    {
        RestaurantData ReadRestaurantData();
    }
    
    public class DataService : IRestaurantDataService
    {
        private readonly string _filePath;

        public DataService(string filePath = "Assets/RestaurantData.json")
        {
            _filePath = filePath;
        }

        

        public RestaurantData ReadRestaurantData()
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"The file {_filePath} was not found.");
            }

            var json = File.ReadAllText(_filePath);



            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<RestaurantData>(json, options);

            if (data is null)
            {
                throw new InvalidDataException("Failed to deserialize restaurant data from JSON.");
            }

            return data;
        }
    }
}