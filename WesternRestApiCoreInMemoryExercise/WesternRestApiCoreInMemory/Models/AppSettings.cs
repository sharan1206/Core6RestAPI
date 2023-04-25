namespace WesternRestApiCoreInMemory.Models
{
    public interface IAppSettings
    {
        public string ExpiryInDays { get; }
    }


    public class AppSettings : IAppSettings
    {
        private IConfiguration _configuration;
        public AppSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ExpiryInDays => _configuration["AppSettings:ExpiryInDays"] ?? throw new NullReferenceException();
    }
}
