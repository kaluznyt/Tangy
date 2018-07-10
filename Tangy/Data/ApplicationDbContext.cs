using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tangy.Areas.Identity.Data;
using Tangy.Models;

namespace Tangy.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Category> Category { get; set; }

        public DbSet<MenuItem> MenuItem{ get; set; }

        public DbSet<SubCategory> SubCategory { get; set; }

        public DbSet<Coupon> Coupon { get; set; }

        public DbSet<TangyUser> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {


        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var attr = property?.PropertyInfo?.GetCustomAttribute<IndexAttribute>();

                    if (attr != null)
                    {
                        var index = entityType.AddIndex(property);
                        index.IsUnique = attr.IsUnique;
                        index.SqlServer().IsClustered = attr.IsClustered;

                    }
                }
            }
            base.OnModelCreating(builder);
        }

    }
}
