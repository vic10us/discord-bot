namespace v10.Games.Dice;

public class DiceGameService : IDiceGameService
{
    public uint GetNextRoll(uint sides = 6)
    {
        if (sides == 0) return 0;
        var result = (uint)new Random().Next(1, (int)sides + 1);
        return result;
    }

    public IEnumerable<uint> GetNextRolls(uint sides = 6, uint numberOfRolls = 2)
    {
        if (numberOfRolls == 0) yield return default;

        for (uint i = 0; i < numberOfRolls; i++)
        {
            yield return sides;
        }
    }
}
