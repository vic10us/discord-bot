namespace v10.Games.Dice;

public interface IDiceGameService
{
    uint GetNextRoll(uint sides = 6);
    IEnumerable<uint> GetNextRolls(uint sides = 6, uint numberOfRolls = 2);
}
