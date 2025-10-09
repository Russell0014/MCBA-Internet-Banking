using Xunit;
using Newtonsoft.Json;
using AdminApi.Helpers;
using AdminApi.Models;

namespace AdminApi.Tests.Helpers
{
    public class AccountTypeConverterTests
    {
        private readonly AccountTypeConverter _converter = new();

        [Fact]
        public void ReadJson_S_returns_Savings()
        {
            var result = JsonConvert.DeserializeObject<AccountType>("\"S\"", _converter);
            Assert.Equal(AccountType.Savings, result);
        }

        [Fact]
        public void ReadJson_C_returns_Checking()
        {
            var result = JsonConvert.DeserializeObject<AccountType>("\"C\"", _converter);
            Assert.Equal(AccountType.Checking, result);
        }

        [Fact]
        public void ReadJson_Invalid_Throws()
        {
            Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<AccountType>("\"X\"", _converter));
        }

        [Fact]
        public void ReadJson_Null_Throws()
        {
            Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<AccountType>("null", _converter));
        }

        [Fact]
        public void WriteJson_Writes_ToString()
        {
            var json = JsonConvert.SerializeObject(AccountType.Savings, _converter);
            Assert.Equal("\"Savings\"", json);
        }
    }
}
