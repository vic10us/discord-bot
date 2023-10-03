namespace v10.Services.Jokes;

[AttributeUsage(AttributeTargets.Class)]
public class ServiceOptionsAttribute : Attribute
{
    public string Section { get; set; }

    public ServiceOptionsAttribute(string section)
    {
        Section = section ?? throw new ArgumentNullException(nameof(section));
    }
}
