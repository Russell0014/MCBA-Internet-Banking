using AdminPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AdminPortal.Controllers;

public class BillPayController : Controller
{
    private readonly HttpClient _client;

    public BillPayController(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient("api");
    }

    public async Task<IActionResult> Index()
    {
        // Update api call to billpay
        using var response = await _client.GetAsync("api/BillPay");

        response.EnsureSuccessStatusCode();

        // Storing the response details received from web api.
        var result = await response.Content.ReadAsStringAsync();

        // Deserializing the response received from web api and storing into a list.
        var billpays = JsonConvert.DeserializeObject<List<BillPayDto>>(result);

        return View(billpays);
    }

    [HttpPost]
    public async Task<IActionResult> BlockBillPay(int id)
    {
        using var content = new StringContent("");
        await _client.PutAsync($"api/BillPay/{id}/block", content);
    
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> UnblockBillPay(int id)
    {
        using var content = new StringContent("");
        await _client.PutAsync($"api/BillPay/{id}/unblock", content);
    
        return RedirectToAction(nameof(Index));
    }
}