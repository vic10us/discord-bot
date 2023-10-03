namespace v10.Bot.Core;

public static class BotLevelingUtils
{
    public static ulong XpNeededForLevel(ulong lvl) => (ulong)(5 * Math.Pow(lvl, 2) + 50 * lvl + 100);
    // public static ulong TotalXpForLevel(ulong level) => (ulong)(5.0f / 6.0f * level * (2 * (ulong)Math.Pow(level,2) + 27 * level + 91));
    public static ulong TotalXpForLevel(ulong x) => (ulong)(5.0f / 6.0f * x * (x + 7.0f) * (2.0f * x + 13.0f));
    public static ulong LevelForTotalXp(ulong totalXp)
    {
        var lvl = (ulong)0;
        var totalXpForCurrentLevel = TotalXpForLevel(lvl + 1);
        while (totalXp >= totalXpForCurrentLevel)
        {
            lvl++;
            totalXpForCurrentLevel = TotalXpForLevel(lvl + 1);
        }
        return lvl;
    }

    // public static ulong LevelForTotalXp(ulong totalXp) => totalXp >= 100 ? 
    //     (ulong)(0.14057f * Math.Pow(1.7321f * Math.Sqrt(3888.0f * Math.Pow(totalXp, 2) + 291600.0f * totalXp - 207025.0f) + 108.0f * totalXp + 4050.0f, 1.0f/3.0f) - 4.5f) + 1
    //     : 0;

    public static (ulong, ulong, ulong, ulong) ComputeLevelAndXp(ulong lvl, ulong xp, Action<ulong> cb = null)
    {
        while (xp >= XpNeededForLevel(lvl))
        {
            xp -= XpNeededForLevel(lvl);
            lvl++;
            if (xp < XpNeededForLevel(lvl)) cb?.Invoke(lvl);
        }
        var next = XpNeededForLevel(lvl);
        var totalXp = TotalXpForLevel(lvl) + xp;
        return (lvl, xp, next, totalXp);
    }
}
