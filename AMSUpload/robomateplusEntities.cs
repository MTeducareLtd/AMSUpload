using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace AMSUpload
{

	public class robomateplusEntities : DbContext
	{
		public virtual DbSet<AzureUploadTracking> AzureUploadTracking { get; set; }

		public virtual DbSet<CourseMaster> CourseMaster { get; set; }

		public virtual DbSet<ProductContentMaster> ProductContentMaster { get; set; }

		public robomateplusEntities()
			: base("name=robomateplusEntities")
		{
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			throw new UnintentionalCodeFirstException();
		}
	}
}