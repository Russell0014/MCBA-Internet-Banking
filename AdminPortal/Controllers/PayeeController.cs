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

    public async Task<IActionResult> Filter([FromQuery] string? postCode)
    {
        if (string.IsNullOrEmpty(postCode))
        {
            // Handle empty search by showing all payees
            return RedirectToAction(nameof(Index));
        }

        using var response = await _client.GetAsync($"api/Payees/filter?postcode={postCode}");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        var payees = JsonConvert.DeserializeObject<List<PayeeDto>>(result);


        return View("Index", payees);
    }

    public async Task<IActionResult> Edit(string id)
    {
        // handle input that aren't numbers
        if (!int.TryParse(id, out var idPayee))
        {
            return RedirectToAction(nameof(Index));
        }

        var response = await _client.GetAsync($"api/Payees/{idPayee}");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        var payee = JsonConvert.DeserializeObject<PayeeDto>(result);

        if (payee == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(payee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PayeeDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var json = JsonConvert.SerializeObject(model);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        using var response = await _client.PutAsync($"api/Payees/{model.PayeeId}", content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }
}