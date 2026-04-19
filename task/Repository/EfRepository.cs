using DataLayer;
using DataLayer.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text.Json;
using task.Infrastructure;
using task.Model.Model;

namespace task.Repository
{
    public interface IDbRepository
    {
        Task<IEnumerable<Office>> GetOfficess();
        Office GetOrder(int officeId);
        void DeleteOfficess();
        void DeleteOfficess(Office[] officess);
        Task ImportOfficess(string source, CancellationToken stoppingToken);
        Task<IEnumerable<Office>> FindListOfOffice(string addressCity, string addressRegion);
        Task<IEnumerable<int>> FindCityCode(string addressCity, string addressRegion);
        /// <summary>
        /// Тест через api загрузки json и десериализации в коллекцию
        /// </summary>
        /// <returns></returns>
        Task<JsonDeserialize> GetJson();
    }

    public class EfRepository(ILogger<EfRepository> logger, IJsonRepository jsonRepository, IServiceScopeFactory serviceScopeFactory) : IDbRepository
    {
        private ILogger<EfRepository> _logger = logger;
        private IJsonRepository _jsonRepository = jsonRepository;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

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
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();

            return await dbContext.Offices.AsNoTracking().ToListAsync();
        }

        public Office GetOrder(int officeId)
        {
            throw new NotImplementedException();
        }

        public async Task ImportOfficess(string source, CancellationToken stoppingToken)
        {
            try
            {
                var jsondata = await _jsonRepository.LoadJson(source);
                foreach (var item in jsondata.city)
                {
                    await Parallel.ForEachAsync(item.terminals.terminal, async (i, stoppingToken) =>
                    {
                        await Task.Run(() =>
                        {
                            using var scope = _serviceScopeFactory.CreateScope();
                            var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();
                            Office office = MapJsonDataToOffice(i, item);
                            office.Phones = [.. i.phones.Select(s=> MapJsonDataToPhone(s, office.Id))];
                            office.Coordinates = MapJsonDataToCoordinates(i, office.Id);

                            dbContext.Offices.Add(office);

                            dbContext.SaveChanges();
                        }, stoppingToken);
                    }).ConfigureAwait(false);
                }
            }
            catch (Exception ex) {
                _logger.LogError($"Ошибка импорта {ex.Message} or {JsonSerializer.Serialize(ex)}");
            }

        }

        public async Task<JsonDeserialize> GetJson()
        {
            return await _jsonRepository.LoadJson("task.files.terminals.json");
        }

        public async Task<IEnumerable<Office>> FindListOfOffice(string addressCity, string addressRegion)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();
            return await dbContext.Offices.Where(w => w.AddressCity.Contains(addressCity) && w.AddressRegion.Contains(addressRegion)).AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<int>> FindCityCode(string addressCity, string addressRegion)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();
            return await dbContext.Offices
                    .Where(w => w.AddressCity.Contains(addressCity) && w.AddressRegion.Contains(addressRegion))
                    .Select(s => s.CityCode)
                    .ToListAsync();
        }

        private Office MapJsonDataToOffice(terminal jsonTreminalData, city jsonCityData)
        {
            var adressStreet = GetHouseNumber(jsonTreminalData.address);
            int.TryParse(jsonTreminalData.id, out int id);

            var result = new Office
            {
                Id = jsonTreminalData.id.ParseAs(0),
                AddressApartment = GetApartment(jsonTreminalData.fullAddress),
                AddressCity = jsonCityData.name,
                AddressHouseNumber = adressStreet.house,
                AddressRegion = string.Empty,
                AddressStreet = adressStreet.street,
                CityCode = jsonCityData.cityID ?? 0,
                Code = string.Empty,
                CountryCode = string.Empty,
                Type = Model.OfficeType.WAREHOUSE,
                Uuid = string.Empty,
                WorkTime = string.Empty
            };

            return result;
        }

        private Phone MapJsonDataToPhone(phone jsonPhoneData, int officeId)
        {
            var result = new Phone
            {
                Additional = jsonPhoneData.type ?? string.Empty,
                OfficeId = officeId,
                PhoneNumber = jsonPhoneData.number ?? string.Empty
            };

            return result;
        }

        private Coordinates MapJsonDataToCoordinates(terminal jsonTreminalData, int officeId)
        {
            var result = new Coordinates
            {
                OfficeId = officeId,
                Latitude = jsonTreminalData.latitude.ParseAs(0.0),
                Longitude = jsonTreminalData.longitude.ParseAs(0.0)
            };

            return result;
        }

        private int? GetApartment(string fullAddres)
        {
            var address = fullAddres.Split(',');
            if (address.Length > 0) {
                var result = address.FirstOrDefault(f => f.Contains("офис"))?.Replace("офис", "");
                int.TryParse(result, out int apartment);
                return apartment;
            }
            return null; 
        }

        public delegate bool TryParseHandler<T>(string value, out T result);

        private (string? street, string? house) GetHouseNumber(string address)
        {
            var addr = address.Split(',');
            if (addr.Length > 1)
            {
                return (addr.Last(), addr.First());
            }
            return (null, null);
        }
    }
}
