namespace v10.Messaging;

public class RedisConfiguration
{
    public string ConnectionStringAdmin => $"{ConnectionString},allowAdmin=true";

    public string ConnectionString { get; set; }

    public override string ToString()
    {
        return $"{ConnectionString}";
    }
}
