namespace AppSettings
{
    public partial class Settings(string? dataFolder, string? dbConnection, bool showProgress)
    {
        public string DataFolder { get; set; } = dataFolder ?? string.Empty;

        public string DbConnection { get; set; } = dbConnection ?? string.Empty;

        public bool ShowProgress { get; set; } = showProgress;
    }
}
