using System;

namespace v10.Data.Abstractions.Models;

public class Background
{
  public Guid Id { get; set; }
  public bool hasImage { get; set; }
  public string image { get; set; }
  public string color { get; set; }
}
