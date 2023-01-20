using CancerPredictionApi.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CancerPredictionApi.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<User> users { get; set; }
    }
}
