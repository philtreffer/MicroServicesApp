using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelpers;

public class MappingProfiles :Profile
{
public MappingProfiles()
{
    //Define where to map from and to
    CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
    CreateMap<Item, AuctionDto>(); // from Item to AuctionDto
    CreateMap<CreateAuctionDto,Auction>() // From Create Auction to Auction 
        .ForMember(d => d.Item, o => o.MapFrom(s => s)); // d = destination, o = object, s = source
    CreateMap<CreateAuctionDto, Item>(); // from Create Auction to item 
    // Don't forget to provide this as a service im Program.cs !
}
}
