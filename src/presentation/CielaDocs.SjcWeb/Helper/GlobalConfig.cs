namespace CielaDocs.SjcWeb.Helper
{
    public static class GlobalConfig
    {
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
