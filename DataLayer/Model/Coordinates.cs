using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Model
{
    public class Coordinates
    {
        public int Id { get; set; }
        public int OfficeId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Office Office { get; set; }
    }

    public class CoordinatesConfiguration : IEntityTypeConfiguration<Coordinates>
    {
        public void Configure(EntityTypeBuilder<Coordinates> builder)
        {
            builder.Property(s => s.Id).ValueGeneratedOnAdd();
        }
    }

}
