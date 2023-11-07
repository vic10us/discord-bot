using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace v10.Data.Abstractions.Models;

public class GuildTypeConverter : TypeConverter
{
    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
    {
        return base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        return base.ConvertTo(context, culture, value, destinationType);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }
        return base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string guildName)
        {

        }
        return base.ConvertFrom(context, culture, value);
    }

    //public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo culture, object value)
    //{
    //    if (value is string guildName)
    //    {
    //        // Implement logic to find the Guild object by name from your guilds list
    //        // Assuming guilds is a List<Guild> in your parent component
    //        // Example: 
    //        // var selectedGuild = guilds.FirstOrDefault(g => g.Name == guildName);
    //        // return selectedGuild;
    //    }
    //    return base.ConvertFrom(context, culture, value);
    //}
}

//[TypeConverter(typeof(GuildTypeConverter))]
public class Guild
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string guildId { get; set; }
    public string guildName { get; set; }
    public IDictionary<string, string> channelNotifications { get; set; } = new Dictionary<string, string>();
    public string[] staffRoles { get; set; } = Array.Empty<string>();
    public bool isInstalled { get; set; } = false;

    public Guild() { }

    public Guild(string id, Guild guild)
    {
        Id = id;
        guildId = guild.guildId;
        guildName = guild.guildName;
        channelNotifications = guild.channelNotifications;
        staffRoles = guild.staffRoles;
        isInstalled = guild.isInstalled;
    }
}
