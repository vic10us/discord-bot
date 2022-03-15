using System;
using System.Collections.Generic;

namespace bot.Features.Games;

public class DiceGame
{
  public uint GetNextRoll(uint sides = 6)
  {
    if (sides == 0) return 0;
    var result = (uint)new Random().Next(1, (int)sides + 1);
    return result;
  }

  public IList<uint> GetNextRolls(uint sides = 6, uint numberOfRolls = 2)
  {
    if (numberOfRolls == 0) return new List<uint>();
    var result = new List<uint>();
    for (uint i = 0; i < numberOfRolls; i++)
    {
      result.Add(GetNextRoll(sides));
    }
    return result;
  }
}
