namespace AppSettings
{
    public class Settings
    {
        // Parameterless ctor required by the configuration binder
        public Settings() {}

        // Properties must be settable so configuration.Bind can populate them
        public string DataFolder { get; set; }
        public string DbConnection { get; set; }
        public bool ShowProgress { get; set; }

        // Bulk processing tuning
        public string? BulkWorkerCount { get; set; }
        public string? BulkBatchSize { get; set; }

        // other properties...
    }
}
