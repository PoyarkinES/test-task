using DataLayer;
using DataLayer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using task.Model.Model;
using task.Repository;

namespace task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficeController(ILogger<OfficeController> logger, IDbRepository dbRepository) : ControllerBase
    {
        private readonly IDbRepository _dbRepository = dbRepository;
        private readonly ILogger<OfficeController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Test()
        {           
            return Ok();
        }

        [HttpGet]
        [Route("getall")]
        public async Task<IEnumerable<Office>> GetOfficess()
        {
            try
            {
                return await _dbRepository.GetOfficess();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return [];
            }
        }

        [HttpGet]
        [Route("getall/{officessId}")]
        public async Task<Office?> GetOfficessById(int officessId)
        {
            try
            {
                return await _dbRepository.GetOffice(officessId); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [HttpGet]
        [Route("findoffice")]
        public async Task<IEnumerable<Office>> FindListOfOffice(string? addressCity, string? addressRegion)
        {
            try
            {
                var result = await _dbRepository.FindListOfOffice(addressCity, addressRegion);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return [];
            }
        }

        [HttpGet]
        [Route("findcitycode")]
        public async Task<int> FindCityCode(string? addressCity, string? addressRegion)
        {
            try
            {
                var result = await _dbRepository.FindCityCode(addressCity, addressRegion);
                if (result.Any())
                {
                    _logger.LogError($"City code {result.Single()}");
                    return result.Single();
                }

                throw new Exception($"City code not found");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return 0;
            }
        }

        [HttpGet]
        [Route("deleteAll")]
        public async Task DeleteAll(CancellationToken stoppingToken)
        {
            try
            {
                await _dbRepository.DeleteOfficess(stoppingToken);
                _logger.LogError("Все офисы удалены.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        [HttpPost]
        [Route("deleteAll/{officeId}")]
        public async Task DeleteById(int officeId, CancellationToken stoppingToken)
        {
            try
            {
                await _dbRepository.DeleteOfficess(officeId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        [HttpPost]
        [Route("deleteAll/officessesId")]
        public async Task DeleteRangeId([FromBody]int[] officessesId, CancellationToken stoppingToken)
        {
            try
            {
                await _dbRepository.DeleteOfficess(officessesId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        [HttpGet]
        [Route("getjson")]
        public async Task<JsonDeserialize?> GetJson()
        {
            try
            {
                return await _dbRepository.GetJson();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
