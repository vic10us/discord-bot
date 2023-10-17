namespace bot;

public class RedisConfiguration
{
    public string ConnectionStringAdmin => $"{this.ConnectionString},allowAdmin=true";

    public string ConnectionString { get; internal set; }

    public override string ToString()
    {
        return $"{ConnectionString}";
    }
}
