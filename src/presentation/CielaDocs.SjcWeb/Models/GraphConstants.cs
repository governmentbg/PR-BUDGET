namespace CielaDocs.Models
{
    public static class GraphConstants
    {
        public readonly static string[] DefaultScopes =
        {
            UserRead
            //UserReadWrite
        };
        // User
        public const string UserRead = "User.Read";
        public const string UserReadBasicAll = "User.ReadBasic.All";
        public const string UserReadAll = "User.Read.All";
        public const string UserReadWrite = "User.ReadWrite";
        public const string UserReadWriteAll = "User.ReadWrite.All";
    }
}
