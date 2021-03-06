namespace Accessioning.IntegrationTests.FeatureTests.Sample
{
    using Accessioning.SharedTestHelpers.Fakes.Sample;
    using Accessioning.IntegrationTests.TestUtilities;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using static Accessioning.WebApi.Features.Samples.DeleteSample;
    using static TestFixture;

    public class DeleteSampleCommandTests : TestBase
    {
        [Test]
        public async Task DeleteSampleCommand_Deletes_Sample_From_Db()
        {
            // Arrange
            var fakeSampleOne = new FakeSample { }.Generate();
            await InsertAsync(fakeSampleOne);
            var sample = await ExecuteDbContextAsync(db => db.Samples.SingleOrDefaultAsync());
            var sampleId = sample.SampleId;

            // Act
            var command = new DeleteSampleCommand(sampleId);
            var sampleReturned = await SendAsync(command);
            await SendAsync(command);
            var samples = await ExecuteDbContextAsync(db => db.Samples.ToListAsync());

            // Assert
            samples.Count.Should().Be(0);
        }
    }
}