using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using task.Model;

namespace DataLayer.Model
{
    public class Office
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public int CityCode { get; set; }
        public string? Uuid { get; set; }
        public OfficeType? Type { get; set; }
        public string CountryCode { get; set; }
        [NotMapped]
        public Coordinates Coordinates { get; set; }
        public string? AddressRegion { get; set; }
        public string? AddressCity { get; set; }
        public string? AddressStreet { get; set; }
        public string? AddressHouseNumber { get; set; }
        public int? AddressApartment { get; set; }
        public string WorkTime { get; set; }
        public Phone Phones { get; set; }
        public Office() { }
    }
}
