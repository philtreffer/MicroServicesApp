using Polly;
using Polly.Extensions.Http;
using SearchService;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddControllers();
// Adding Search Service from Lesson 31
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();
app.Lifetime.ApplicationStarted.Register(async()=>{

try
{
    await DbInitializer.InitDb(app);
}
catch (Exception e)
{
    
    System.Console.WriteLine(e);
}


});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_=> TimeSpan.FromSeconds(3));
