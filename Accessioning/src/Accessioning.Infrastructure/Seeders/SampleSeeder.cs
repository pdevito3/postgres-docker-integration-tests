namespace Accessioning.Infrastructure.Seeders
{

    using AutoBogus;
    using Accessioning.Core.Entities;
    using Accessioning.Infrastructure.Contexts;
    using System.Linq;

    public static class SampleSeeder
    {
        public static void SeedSampleSampleData(AccessioningDbContext context)
        {
            if (!context.Samples.Any())
            {
                context.Samples.Add(new AutoFaker<Sample>());
                context.Samples.Add(new AutoFaker<Sample>());
                context.Samples.Add(new AutoFaker<Sample>());

                context.SaveChanges();
            }
        }
    }
}