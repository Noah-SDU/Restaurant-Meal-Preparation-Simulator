using System.Text.Json;
using RestaurantSimulator.Services;

namespace Test;

public class UnitTest1
{
    [Fact]
    public void ReadRestaurantData_WhenFileMissing_ThrowsFileNotFoundException()
    {
        // Arrange
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        var sut = new DataService(missingPath);

        // Act + Assert
        Assert.Throws<FileNotFoundException>(() => sut.ReadRestaurantData());
    }

    [Fact]
    public void ReadRestaurantData_WhenJsonInvalid_ThrowsJsonException()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        File.WriteAllText(path, "this is not valid json");
        var sut = new DataService(path);

        try
        {
            // Act + Assert
            Assert.Throws<JsonException>(() => sut.ReadRestaurantData());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReadRestaurantData_WhenJsonValid_ReturnsDeserializedData()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        // Minimal JSON that matches RestaurantData shape
        var json = """
                   {
                     "stations": [],
                     "ingredients": [],
                     "recipes": []
                   }
                   """;

        File.WriteAllText(path, json);
        var sut = new DataService(path);

        try
        {
            // Act
            var data = sut.ReadRestaurantData();

            // Assert
            Assert.NotNull(data);
            Assert.NotNull(data.Stations);
            Assert.NotNull(data.Ingredients);
            Assert.NotNull(data.Recipes);
            Assert.Empty(data.Stations);
            Assert.Empty(data.Ingredients);
            Assert.Empty(data.Recipes);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
