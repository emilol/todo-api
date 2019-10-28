namespace TodoApi.Data
{
    public class DatabaseSettings
    {
        public bool UseInMemoryDatabase { get; set; }
        public string ConnectionStringName { get; set; }
        public string DatabaseName { get; set; }
    }
}