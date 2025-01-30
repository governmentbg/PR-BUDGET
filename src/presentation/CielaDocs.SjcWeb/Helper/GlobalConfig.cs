namespace CielaDocs.SjcWeb.Helper
{
    public static class GlobalConfig
    {
        public const decimal EuroRate = 1.95583M;
        public static IConfiguration Configuration { get; private set; }

        public static void Initialize(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static string GetValue(string key)
        {
            return Configuration?[key];
        }
    }
}
