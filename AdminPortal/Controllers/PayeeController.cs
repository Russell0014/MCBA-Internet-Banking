using AdminPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AdminPortal.Controllers;

public class PayeeController : Controller
{
    
    private readonly HttpClient _client;

    public PayeeController(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient("api");
    }

    public async Task<IActionResult> Index()
    {
        using var response = await _client.GetAsync("api/Payees");

        response.EnsureSuccessStatusCode();

        // Storing the response details received from web api.
        var result = await response.Content.ReadAsStringAsync();

        // Deserializing the response received from web api and storing into a list.
        var payees = JsonConvert.DeserializeObject<List<PayeeDto>>(result);

        return View(payees);
    }
}