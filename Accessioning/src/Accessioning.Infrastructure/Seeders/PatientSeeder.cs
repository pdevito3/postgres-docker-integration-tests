namespace Accessioning.Infrastructure.Seeders
{

    using AutoBogus;
    using Accessioning.Core.Entities;
    using Accessioning.Infrastructure.Contexts;
    using System.Linq;

    public static class PatientSeeder
    {
        public static void SeedSamplePatientData(AccessioningDbContext context)
        {
            if (!context.Patients.Any())
            {
                context.Patients.Add(new AutoFaker<Patient>());
                context.Patients.Add(new AutoFaker<Patient>());
                context.Patients.Add(new AutoFaker<Patient>());

                context.SaveChanges();
            }
        }
    }
}