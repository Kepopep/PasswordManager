using Core.Source;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        [HttpGet("Generate")]
        public string? Generate([FromQuery]GenerateRequestModel data)
        {
            var filter = ConfigureFilter(data);
            
            return PasswordGenerator.GenerateSingle(data.Length, filter);
        }
        
        [HttpGet("GeneratePack")]
        public List<string?> GeneratePack([FromQuery]GenerateRequestModel data)
        {
            var filter = ConfigureFilter(data);
            
            return PasswordGenerator.GeneratePack(data.Length, filter: filter) ?? [];
        }

        private PasswordGenerator.Filter ConfigureFilter(GenerateRequestModel data)
        {
            var filter = PasswordGenerator.Filter.Lowercase;

            if (data.UseUppercase)
                filter |= PasswordGenerator.Filter.Uppercase;
            
            if (data.UseNumber)
                filter |= PasswordGenerator.Filter.Number;
            
            if (data.UseSpecific)
                filter |= PasswordGenerator.Filter.Specific;

            return filter;
        }
    }
}
