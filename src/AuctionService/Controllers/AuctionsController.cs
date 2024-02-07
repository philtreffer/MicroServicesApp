using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController] // Checks for required Props, returns bad request, when it fails validation, 
                //also bind properties that are given as arguments to our API endpoints as well
[Route("api/auctions")]
public class AuctionsController :ControllerBase
{// This Controller only has end points, no view
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

            //  THis Controller is retunring the data in JSON Format from these API End Points

            // This is our first cTor(constructor) which uses dependency injection
            // Services we defined in Program.cs can now be used in this class
            // We need to use our dbcontext so that we can access the data and
            //we need to access to automapper so that we can shape the data and automatically map it from the auction
    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {       // Use quick fix command + "." to create and design field context ( automatically creates provate readonly field and this.context ...) 
        _context = context;
        _mapper = mapper;
            /*And the way that dependency injection works is that when our framework creates a new instance of the
            auctions controller, which it will do when it receives a request into this particular route, then
            it's going to take a look at the arguments inside the controller and it's going to say, Right, okay,
            I see you want a dbcontext and a mapper and it's going to instantiate these classes and make them available
            inside here.*/
   }

            // Now that we have that, let's create a couple of endpoints 

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
            //Adjustment from Lesson 31 for adding synchronos Messaging
            var query =_context.Auctions.OrderBy(x => x.Item.Make).AsQueryable(); // .AsQueryable is neccesary, so that we can perform other queries too ( we dont want it to stick it to the type OrderBy)

            if(!string.IsNullOrEmpty(date))
            {// Only returning auction, which are after a particular date
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime())>0);// CompareTo: x < 0: earlier , x > o: later
            }
                // New approach of returning auctions in Lesson 31

                return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
                 // Removed after lesson 31, due to a better solution above
                /*
                var auctions = await _context.Auctions
                    .Include(x => x.Item)
                    .OrderBy(x => x.Item.Make)
                    .ToListAsync();
                    return _mapper.Map<List<AuctionDto>>(auctions);
                */

    }
    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        
       
        var auction = await _context.Auctions 
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id ==id);
        if (auction ==null) return NotFound();
        return _mapper.Map<AuctionDto>(auction);
       
    }
        /*And we don't need to specify any parameters for this because our framework is smart enough to realize that if a post request comes in to list routes, then this is an Http get request.*/
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction =_mapper.Map<Auction>(auctionDto);
        //TODO: Add current user as seller
        auction .Seller="test";
        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() >0;
        if(!result) return BadRequest("Could not save changes to the DB");
        return CreatedAtAction(nameof(GetAuctionById), 
            new{auction.Id}, _mapper.Map<AuctionDto>(auction) );


    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {

        var auction = await _context.Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        
        if(auction ==null) return NotFound();
        //TODO: check seller == username

        auction.Item.Make=updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model=updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color=updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage=updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year= updateAuctionDto.Year ?? auction.Item.Year;

        var result= await _context.SaveChangesAsync()>0;
        if(result) return Ok();
        return BadRequest("Problem saving changes");


    }
    // Not sure if Auction Delete is allowed in the final product
    // TODO: Maybe only for Admins
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        //Get the auction from the Database
        var auction= await _context.Auctions.FindAsync(id);
        
        if(auction== null) return NotFound();
        //TODO: Seller = username

        _context.Auctions.Remove(auction);

        var result = await _context.SaveChangesAsync()>0;
        if(!result) return BadRequest("Could not delete and update DB");
        
        return Ok();



    }
}