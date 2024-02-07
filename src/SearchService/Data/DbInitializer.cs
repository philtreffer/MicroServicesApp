using System.Text.Json;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MongoDB.Driver;
using MongoDB.Entities;
namespace SearchService;

public class DbInitializer
{
public static async Task InitDb(WebApplication app ){
    await DB.InitAsync("SearchDb", MongoClientSettings
        .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

    await DB.Index<Item>()
        .Key(x => x.Make, KeyType.Text)
        .Key(x => x.Model, KeyType.Text)
        .Key(x => x.Color, KeyType.Text)
        .CreateAsync();
    var count = await DB.CountAsync<Item>();

    // New approach from Lessson 31 using Http Service to get the data
        using var scope = app.Services.CreateScope();
        
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();

        var items = await httpClient.GetItemsForSearchDb();
        System.Console.WriteLine( items.Count+ "returned from the auctionService");
        if(items.Count>0) await DB.SaveAsync(items);

    // Getting rid of this code in Lesson 31 to read data from a file
    /*
    if(count==0)
    {
        System.Console.WriteLine("No Data - will attempt to seed");
        // temporary solution in Section 3
        var itemData = await File.ReadAllTextAsync("Data/auctions.json");

        var options = new JsonSerializerOptions{PropertyNameCaseInsensitive =true};

        var items =JsonSerializer.Deserialize<List<Item>>(itemData, options);

        await DB.SaveAsync(items);

    }
    */
    }
}
