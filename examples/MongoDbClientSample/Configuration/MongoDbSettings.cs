namespace MongoDbClientSample.Configuration;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "sample_db";
    public string CollectionName { get; set; } = "sample_collection";
}
