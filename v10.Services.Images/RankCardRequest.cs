namespace v10.Services.Images;

public class RankCardRequest
{
    public int rank;
    public string userDescriminator;
    public string cardTitle;
    public string userName { get; set; }
    public string avatarUrl { get; set; }
    public int textXp { get; set; }
    public int voiceXp { get; set; }
    public int textLevel { get; set; }
    public int voiceLevel { get; set; }
    public int xpForNextTextLevel { get; set; }
    public int xpForNextVoiceLevel { get; set; }
}
