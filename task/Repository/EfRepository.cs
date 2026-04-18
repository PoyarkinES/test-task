using DataLayer;
using DataLayer.Model;
using System.Data.Entity;
using System.Reflection;
using System.Threading.Tasks;

namespace task.Repository
{
    public interface IDbRepository
    {
        Task<IEnumerable<Office>> GetOfficess();
        Office GetOrder(int officeId);
        void DeleteOfficess();
        void DeleteOfficess(Office[] officess);
        Task ImportOfficess(string source);
        Task<IEnumerable<Office>> FindListOfOffice(string addressCity, string addressRegion);
        Task<IEnumerable<int>> FindCityCode(string addressCity, string addressRegion);
        /// <summary>
        /// Тест через api загрузки json и десериализации в коллекцию
        /// </summary>
        /// <returns></returns>
        Task<JsonDeserialize> GetJson();
    }

    public class EfRepository(IJsonRepository jsonRepository, DellinDictionaryDbContext dbContext) : IDbRepository
    {
        private IJsonRepository _jsonRepository = jsonRepository;
        private DellinDictionaryDbContext _dbContext = dbContext;

        public void DeleteOfficess()
        {
            throw new NotImplementedException();
        }

        public void DeleteOfficess(Office[] officess)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Office>> GetOfficess()
        {
            return await _dbContext.Offices.AsNoTracking().ToListAsync();
        }

        public Office GetOrder(int officeId)
        {
            throw new NotImplementedException();
        }

        public async Task ImportOfficess(string source)
        {
            var jsondata = await _jsonRepository.LoadJson(source);
            _dbContext.
        }

        public async Task<JsonDeserialize> GetJson()
        {
            return await _jsonRepository.LoadJson("task.files.terminals.json");
        }

        public async Task<IEnumerable<Office>> FindListOfOffice(string addressCity, string addressRegion)
        {
            return await _dbContext.Offices.Where(w => w.AddressCity.Contains(addressCity) && w.AddressRegion.Contains(addressRegion)).AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<int>> FindCityCode(string addressCity, string addressRegion)
        {
            return await _dbContext.Offices
                    .Where(w => w.AddressCity.Contains(addressCity) && w.AddressRegion.Contains(addressRegion))
                    .Select(s => s.CityCode)
                    .ToListAsync();
        }

        //public static MapJsonDataToOffice(city jsonCityData, terminal jsonTreminalData)
        //{
        //    var result = new Office
        //    {
        //        AddressApartment = ,
        //        AddressCity = ,
        //        AddressHouseNumber = ,
        //        AddressRegion = ,
        //        AddressStreet = ,
        //        CityCode = ,
        //        Code = ,
        //        Coordinates = ,
        //        CountryCode = ,
        //        Id = ,
        //        Phones = ,
        //        Type = ,
        //        Uuid = ,
        //        WorkTime = 
        //    };

        //    return result;
        //}
    }
}
