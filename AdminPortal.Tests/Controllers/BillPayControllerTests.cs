using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using AdminPortal.Controllers;
using AdminPortal.Models;
using AdminPortal.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit;

namespace AdminPortal.Tests.Controllers;

public class BillPayControllerTests
{
    [Fact]
    public async void Index_ReturnsViewWithBillPays_OnSuccess()
    {
        var list = new List<BillPayDto> { new BillPayDto { BillPayId = 1, AccountNumber = 1, PayeeId = 1, Amount = 10m, ScheduleTimeUtc = System.DateTime.UtcNow, Period = BillPayDto.PeriodType.OneOff, Status = BillPayDto.StatusType.Pending } };

        var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(list), Encoding.UTF8, "application/json")
        });

        var client = new HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/") };
        var controller = new BillPayController(new TestHttpClientFactory(client));

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<BillPayDto>>(view.Model);
        Assert.Single(model);
    }

    [Fact]
    public async void BlockBillPay_Post_RedirectsToIndex()
    {
        var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
        var client = new HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/") };
        var controller = new BillPayController(new TestHttpClientFactory(client));

        var result = await controller.BlockBillPay(5);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async void UnblockBillPay_Post_RedirectsToIndex()
    {
        var handler = new TestHttpMessageHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
        var client = new HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/") };
        var controller = new BillPayController(new TestHttpClientFactory(client));

        var result = await controller.UnblockBillPay(5);

        Assert.IsType<RedirectToActionResult>(result);
    }
}
