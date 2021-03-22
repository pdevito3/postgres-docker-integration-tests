namespace Accessioning.IntegrationTests
{
    using NUnit.Framework;
    using System.Threading.Tasks;
    using static TestFixture;

    public class TestBase
    {
        [SetUp]
        public async Task TestSetUp()
        {
            await ResetState();
        }
    }
}