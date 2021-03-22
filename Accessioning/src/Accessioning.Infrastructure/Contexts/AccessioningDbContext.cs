namespace Accessioning.Infrastructure.Contexts
{
    using Accessioning.Core.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using System.Threading;
    using System.Threading.Tasks;

    public class AccessioningDbContext : DbContext
    {
        public AccessioningDbContext(
            DbContextOptions<AccessioningDbContext> options) : base(options) 
        {
        }

        #region DbSet Region - Do Not Delete
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Sample> Samples { get; set; }
        #endregion
    }
}