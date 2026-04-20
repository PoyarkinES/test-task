using DataLayer;
using DataLayer.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using System.Linq;
using System.Text.Json;
using task.Infrastructure;
using task.Model.Model;

namespace task.Repository
{
    public interface IDbRepository
    {
        Task<IEnumerable<Office>> GetOfficess();
        Task<Office?> GetOffice(int officeId);
        Task DeleteOfficess(CancellationToken stoppingToken);
        Task DeleteOfficess(int officess, CancellationToken stoppingToken);
        Task DeleteOfficess(int[] officesses, CancellationToken stoppingToken);
        Task ImportOfficess(string source, CancellationToken stoppingToken);
        Task<IEnumerable<Office>> FindListOfOffice(string? city, string? region);
        Task<IEnumerable<int>> FindCityCode(string? city, string? region);
        /// <summary>
        /// Тест через api для проверки загрузки json и десериализации в коллекцию
        /// </summary>
        /// <returns></returns>
        Task<JsonDeserialize> GetJson();
    }

    public class EfRepository(ILogger<EfRepository> logger, IJsonRepository jsonRepository, IServiceScopeFactory serviceScopeFactory) : IDbRepository
    {
        private ILogger<EfRepository> _logger = logger;
        private IJsonRepository _jsonRepository = jsonRepository;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

        public async Task DeleteOfficess(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();
                    var deleted = dbContext.Offices.Count();
                    dbContext.Offices.RemoveRange(dbContext.Offices);
                    _logger.LogInformation("Удалено {deleted} терминалов.", deleted);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Ошибка удаления {Message}", ex.Message);
                }
            }, stoppingToken);
        }

        public async Task DeleteOfficess(int officess, CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();
                    var deleted = dbContext.Offices.FirstOrDefault(f => f.Id == officess);
                    if (deleted != null)
                    {
                        dbContext.Offices.Remove(deleted);
                        _logger.LogInformation("Удален 1 терминал Id: {Id}.", officess);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Ошибка удаления {Message}", ex.Message);
                }
            }, stoppingToken);
        }

        public async Task DeleteOfficess(int[] officesses, CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();
                    var deleted = dbContext.Offices.Where(f => officesses.Contains(f.Id)).ToArray();
                    if (deleted.Length > 0)
                    {
                        dbContext.Offices.RemoveRange(deleted);
                        _logger.LogInformation("Удалено {Length} терминалов.", deleted.Length);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Ошибка удаления {Message}", ex.Message);
                }
            }, stoppingToken);
        }

        public async Task<IEnumerable<Office>> GetOfficess()
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();

                _logger.LogError("Выгрузки данных об офисах завершена.");
                return await dbContext.Offices.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка выгрузки данных об офисах {Message}", ex.Message);
                return [];
            }
        }

        public async Task<Office?> GetOffice(int officeId)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();
                _logger.LogError("Выгзука данных по офису {officeId} завершена.", officeId);
                return await dbContext.Offices.FirstOrDefaultAsync(f => f.Id == officeId);
            }
            catch (Exception ex) {
                _logger.LogError("Ошибка выгрузки данных об офисе Id: {officeId}  error: {Message}", officeId, ex.Message);
                return null;
            }
        }

        public async Task ImportOfficess(string source, CancellationToken stoppingToken)
        {
            try
            {
                var jsondata = await _jsonRepository.LoadJson(source);
                var loadedcount = 0;
                await Parallel.ForEachAsync(jsondata.city, async (i, stoppingToken) =>
                {
                    await Task.Run(() =>
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();
                        dbContext.Database.BeginTransaction();
                        try
                        {
                            Office[] office = [.. i.terminals.terminal.Where(w => !dbContext.Offices.Any(a => a.Id == w.id.ParseAs(0))).Select(s => MapJsonDataToOffice(s, i))];

                            dbContext.Offices.AddRangeAsync(office);
                            dbContext.SaveChanges();
                            dbContext.Database.CommitTransaction();
                            loadedcount = loadedcount + office.Length;
                        }
                        catch (Exception ex) 
                        {
                            _logger.LogError("Ошибка импорта {message}", ex.Message);
                            dbContext.Database.RollbackTransaction();
                        }
                    }, stoppingToken);
                }).ConfigureAwait(false);

                _logger.LogInformation("Загружено {loadedcount} терминалов из JSON", loadedcount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка десериализации {Message}", ex.Message);
            }

        }

        public async Task<JsonDeserialize> GetJson()
        {
            return await _jsonRepository.LoadJson("task.files.terminals.json");
        }

        public async Task<IEnumerable<Office>> FindListOfOffice(string? city, string? region)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();
            var result = await dbContext.Offices
                .Where(w =>
                    (city == "null" || string.IsNullOrEmpty(city) || w.AddressCity.Contains(city)) && 
                    (region == "null" || string.IsNullOrEmpty(region) || w.AddressRegion.Contains(region)))
                .AsNoTracking().ToListAsync();

            return result;
        }

        public async Task<IEnumerable<int>> FindCityCode(string? city, string? region)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DellinDictionaryDbContext>();
            var result = await dbContext.Offices
                .Where(w =>
                    (city.Contains("null", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(city) || w.AddressCity.Contains(city, StringComparison.OrdinalIgnoreCase)) &&
                    (region.Contains("null", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(region) || w.AddressRegion.Contains(region, StringComparison.OrdinalIgnoreCase)))
                .Select(s => s.CityCode)
                .ToListAsync();

            
            return result;
        }

        private Office MapJsonDataToOffice(terminal jsonTreminalData, city jsonCityData)
        {
            var adressStreet = GetHouseNumber(jsonTreminalData.address);
            _ = int.TryParse(jsonTreminalData.id, out int id);

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
                WorkTime = string.Empty,
                Coordinates = MapJsonDataToCoordinates(jsonTreminalData, jsonTreminalData.id.ParseAs(0)),
                Phones = [.. jsonTreminalData.phones.Select(s=> MapJsonDataToPhone(s, jsonTreminalData.id.ParseAs(0)))]
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

        private bool checkNull(string? s1, string? s2)
        {
            if (s1.Length == 0)
                return true;
            if (string.IsNullOrWhiteSpace(s1))
                return true;
            if( s2.Contains(s1))
                return true;
            return false;
        }
    }
}
