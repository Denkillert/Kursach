using Microsoft.Extensions.Configuration;
using PolesSU_Sports.Lib.DB;

namespace PolesSU_Sports.Web.Services
{
    public class WebDbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public WebDbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }
    }
}