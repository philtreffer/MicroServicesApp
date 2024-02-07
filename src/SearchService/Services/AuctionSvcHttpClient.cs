﻿using MongoDB.Entities;

namespace SearchService;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }
    public async Task<List<Item>> GetItemsForSearchDb()
    {   // Providing the latest updated Auction in our database
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(xa => xa.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteAnyAsync();
        return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"]
                + "/api/auctions?date=" + lastUpdated);// QueryString, just as simple example

    }

}