using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using AdminPortal.Controllers;
using AdminPortal.Models;
using AdminPortal.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit;

namespace AdminPortal.Tests.Controllers;

public class PayeeControllerTests
{
    [Fact]
    public async void Index_ReturnsViewWithPayees_OnSuccess()
    {
        var payees = new List<PayeeDto> { new PayeeDto { PayeeId = 1, Name = "A", State = "VIC", Postcode = "3000", Phone = "(03) 1234 5678", Address = "x", City = "y" } };
        var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(payees), Encoding.UTF8, "application/json")
        });

        var client = new HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/") };
        var factory = new TestHttpClientFactory(client);

        var controller = new PayeeController(factory);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<PayeeDto>>(view.Model);
        Assert.Single(model);
    }

    [Fact]
    public async void Filter_EmptyPostcode_RedirectsToIndex()
    {
        var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
        var client = new HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/") };
        var controller = new PayeeController(new TestHttpClientFactory(client));

        var result = await controller.Filter(null);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async void Edit_InvalidId_RedirectsToIndex()
    {
        var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
        var client = new HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/") };
        var controller = new PayeeController(new TestHttpClientFactory(client));

        var result = await controller.Edit("not-a-number");

        Assert.IsType<RedirectToActionResult>(result);
    }
}
