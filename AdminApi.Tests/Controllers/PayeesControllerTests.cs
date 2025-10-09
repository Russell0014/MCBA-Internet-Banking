using AdminApi.Models;
using AdminApi.Models.Repository;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.Tests.Controllers
{
    public class PayeesControllerTests
    {
        private class FakeRepo : IPayeeRepository
        {
            private readonly List<Payee> _data = new()
            {
                new Payee{ PayeeId=1, Name = "A", Postcode = "3000" },
                new Payee{ PayeeId=2, Name = "B", Postcode = "3001" }
            };

            public Task<IEnumerable<Payee>> GetAllAsync() => Task.FromResult((IEnumerable<Payee>)_data);
            public Task<IEnumerable<Payee>> GetByPostcodeAsync(string postcode) => Task.FromResult(_data.Where(p => p.Postcode.Contains(postcode)) as IEnumerable<Payee>);
            public Task<Payee?> GetByIdAsync(int id) => Task.FromResult(_data.FirstOrDefault(p => p.PayeeId == id));
            public Task UpdateAsync(Payee payee)
            {
                var existing = _data.FirstOrDefault(p => p.PayeeId == payee.PayeeId);
                if (existing != null)
                {
                    existing.Name = payee.Name;
                }
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task GetPayeesByPostcode_Returns_Matching()
        {
            var controller = new PayeesController(new FakeRepo());
            var action = await controller.GetPayeesByPostcode("300");
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<Payee>>(ok.Value);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public async Task GetPayee_Returns_Payee()
        {
            var controller = new PayeesController(new FakeRepo());
            var action = await controller.GetPayee(1);
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var payee = Assert.IsType<Payee>(ok.Value);
            Assert.Equal(1, payee.PayeeId);
        }
    }
}
