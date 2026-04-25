using System.IO;


namespace RestaurantSimulator.Services
{
    public class DataService
    {
        private readonly string _filePath = "Assets/Recipes.json";

        public DataService(string filePath)
        {
            _filePath = filePath;
        }

        

        public string ReadData()
        {
            if (!File.Exists(_filePath))
            {
                throw new FileNotFoundException($"The file {_filePath} was not found.");
            }

            return File.ReadAllText(_filePath);
        }
    }
}