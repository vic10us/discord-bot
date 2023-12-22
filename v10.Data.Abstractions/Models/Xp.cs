namespace v10.Data.Abstractions.Models;

public class Xp
{
    public int total { get; set; }
    public ulong current { get; set; }
    public ulong required { get; set; }

    public float percent
    {
        get
        {
            return current / (float)required;
        }
    }

    public int progress
    {
        get
        {
            return (int)Math.Max(18, percent * 316);
        }
    }
}
