using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Model
{
    public class Phone
    {
        public int Id { get; set; }

        public int OfficeId { get; set; }

        public string PhoneNumber { get; set; }

        public string? Additional { get; set; }

        public Office Office { get; set; }
    }

    public class PhoneConfiguration : IEntityTypeConfiguration<Phone>
    {
        public void Configure(EntityTypeBuilder<Phone> builder)
        {
            builder.Property(s => s.Id).ValueGeneratedOnAdd();
        }
    }

}
