using System;
using System.Threading.Tasks;
using Xunit;

namespace Anilist.Client.Tests
{
    public class UnitTest1
    {
        private readonly AnilistClient client = new AnilistClient(new System.Net.Http.HttpClient());

        [Fact]
        public async Task Test1()
        {
            var res = await client.SearchMediaAsync("higurashi");
            Assert.NotEmpty(res);
        }
    }
}
