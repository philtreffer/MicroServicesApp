using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using ZstdSharp.Unsafe;

namespace SearchService;

[ApiController]
[Route("api/search")]
public class SearchControllers : ControllerBase
{
    // no constructor needed nor inject mondoDb Entities, due to the fact that it is a static class, we can just use it
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearachItems([FromQuery]SearchParams searchParams) // need to tell API Controller to explicity look at Query
    {
        /*Without using Pagniating the results
        var query = DB.Find<Item>();
        query.Sort(x => x.Ascending( a=> a.Make));
        */
        // Implementation using Paginating the results (lesson 28)
        // We need to specify Item twice in the type parameter to perform query OrderBy 

        var query = DB.PagedSearch<Item, Item>();// Step 1: we create a query, which is a page search 

        //query.Sort(x => x.Ascending( a=> a.Make)); This Line is set as default in the query Orderby

        if(!string.IsNullOrEmpty(searchParams.searchTerm))// Step 2: We check if we have a searchTerm
        {
            query.Match(Search.Full, searchParams.searchTerm).SortByTextScore();//Step 3: If we do, we gonna match it
        }
        
        // adding filters according SearchParams.cs
        // adding filter OrderBy
        query =searchParams.Orderby switch
        {
            "make"=> query.Sort(x => x.Ascending(a => a.Make)),
            "new"=> query.Sort(x => x.Ascending(a => a.CreatedAt)),
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))// Setting a defaul paramter beginning with " _ "
        };
        // adding next filter FilterBy
        query =searchParams.FilterBy switch
        {
            "finished"=> query.Match(x => x.AuctionEnd < DateTime.UtcNow),
            "endingSoon"=> query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) 
            && x.AuctionEnd > DateTime.UtcNow),
            _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow )// Setting a defaul paramter beginning with " _ "
        };
        if(!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);

        }
         if(!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner  == searchParams.Winner);

        }

        // Output Paging the result before returning them 
        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);
        var result = await query.ExecuteAsync();
        return Ok(new{
            results =result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });


    }




}
