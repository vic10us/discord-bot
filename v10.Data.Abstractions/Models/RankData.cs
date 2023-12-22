using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace v10.Data.Abstractions.Models;

public class RankData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public ulong rank { get; set; }
    public ulong level { get; set; }
    public ulong voiceLevel { get; set; }
    public string name { get; set; }
    public string code { get; set; }
    public Xp xp { get; set; }
    public Xp voiceXp { get; set; }
    public Avatar avatar { get; set; }
    public string backgroundColor { get; set; }
    public string levelColor { get; set; }
    public string voiceLevelColor { get; set; }
    public string statusColor { get; set; }
    public string progressBarColor { get; set; }
    public string progressBarBackgroundColor { get; set; }
    public string rankColor { get; set; }
    public string fontColor { get; set; }
    public string codeColor { get; set; }
    public string requiredXPColor { get; set; }
    public Card card { get; set; }
    [BsonExtraElements]
    public BsonDocument Metadata { get; set; }

    public RankData()
    {
        rank = int.MaxValue;
        level = 0;
        voiceLevel = 0;
        name = "UNKNOWN";
        code = "0000";
        xp = new Xp
        {
            current = 0,
            required = 0
        };
        voiceXp = new Xp
        {
            current = 0,
            required = 0
        };
        avatar = new Avatar
        {
            image = "",
            backgroundColor = "black"
        };
        backgroundColor = "#23272A";
        levelColor = "#62D3F5";
        voiceLevelColor = "#62D3F5";
        statusColor = "#747F8D";
        progressBarColor = "#62D3F5";
        progressBarBackgroundColor = "#7F8384";
        rankColor = "white";
        fontColor = "white";
        codeColor = "#7F8384";
        requiredXPColor = "#7F8384";
        card = new Card
        {
            background = new Background
            {
                color = "#23272A",
                image = "",
                hasImage = false
            },
            content = new Content
            {
                background = new ColorSettings
                {
                    color = "black",
                    opacity = 0.3f
                }
            }
        };
    }
}
