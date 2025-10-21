using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class AppConfig
    {
        private static IConfiguration _configuration;
        static AppConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();
        }
        public static string ApiBaseUrl => _configuration["ApiSettings:BaseUrl"];
        public static string DataFolder => _configuration["Offline:DataFolder"];
        public static string QuestionFile => _configuration["Offline:QuestionFile"];
        public static string SubjectFile => _configuration["Offline:SubjectFile"];
        public static string ClassFile => _configuration["Offline:ClassFile"];
    }
}
