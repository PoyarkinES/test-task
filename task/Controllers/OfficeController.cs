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
        public async Task<IEnumerable<Office>> GetOfficess(string addressCity, string addressRegion)
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
        [Route("findoffice/{addressCity}/{addressRegion}")]
        public async Task<IEnumerable<Office>> FindListOfOffice(string addressCity, string addressRegion)
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
        [Route("findcitycode/{addressCity}/{addressRegion}")]
        public async Task<int> FindCityCode(string addressCity, string addressRegion)
        {
            try
            {
                var result = await _dbRepository.FindCityCode(addressCity, addressRegion);
                if (!result.Any())
                {
                    _logger.LogError($"City code {result.Single()}");
                    return result.Single();
                }

                throw new Exception($"Dublicate city code {JsonSerializer.Serialize(result)}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return 0;
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
