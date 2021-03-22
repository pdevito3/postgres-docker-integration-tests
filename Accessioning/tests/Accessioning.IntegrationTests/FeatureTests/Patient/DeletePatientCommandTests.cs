namespace Accessioning.IntegrationTests.FeatureTests.Patient
{
    using Accessioning.SharedTestHelpers.Fakes.Patient;
    using Accessioning.IntegrationTests.TestUtilities;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using static Accessioning.WebApi.Features.Patients.AddPatient;
    using static TestFixture;

    public class DeletePatientCommandTests : TestBase
    {
        [Test]
        public async Task DeletePatientCommand_Deletes_Patient_From_Db()
        {
            // Arrange
            var fakePatientOne = new FakePatient { }.Generate();
            await InsertAsync(fakePatientOne);
            var patient = await ExecuteDbContextAsync(db => db.Patients.SingleOrDefaultAsync());
            var patientId = patient.PatientId;

            // Act
            var command = new DeletePatientCommand(patientId);
            var patientReturned = await SendAsync(command);
            await SendAsync(command);
            var patients = await ExecuteDbContextAsync(db => db.Patients.ToListAsync());

            // Assert
            patients.Count.Should().Be(0);
        }
    }
}