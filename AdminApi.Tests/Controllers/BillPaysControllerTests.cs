using AdminApi.Models;
using AdminApi.Models.Repository;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Tests.Controllers
{
    public class BillPaysControllerTests
    {
        private class SuccessRepo : IBillPayRepository
        {
            public Task<IEnumerable<BillPay>> GetAllAsync() => Task.FromResult((IEnumerable<BillPay>)new List<BillPay>());
            public Task UpdateBillStatusAsync(int billPayId, StatusType status) => Task.CompletedTask;
        }

        private class ErrorRepo : IBillPayRepository
        {
            public Task<IEnumerable<BillPay>> GetAllAsync() => Task.FromResult((IEnumerable<BillPay>)new List<BillPay>());
            public Task UpdateBillStatusAsync(int billPayId, StatusType status) => throw new InvalidOperationException("bad state");
        }

//test that blocking a bill returns NoContent on success
        [Fact]
        public async Task BlockBill_Returns_NoContent_OnSuccess()
        {
            var controller = new BillPayController(new SuccessRepo());
            var result = await controller.BlockBill(1);
            Assert.IsType<NoContentResult>(result);
        }

//test that blocking a bill returns BadRequest on repository invalid operation
        [Fact]
        public async Task BlockBill_Returns_BadRequest_OnRepoInvalidOperation()
        {
            var controller = new BillPayController(new ErrorRepo());
            var result = await controller.BlockBill(1);
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
