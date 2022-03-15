using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace bot.Features.Database;

public class BotDbContext : DbContext
{
  public DbSet<MessageGroup> Groups { get; set; }
  public DbSet<BotReply> Replies { get; set; }
  // protected override void OnConfiguring(DbContextOptionsBuilder options)
  //     => options.UseSqlite(@"Data Source=C:\blogging.db");

  public BotDbContext(DbContextOptions<BotDbContext> options)
      : base(options)
  { }
}

public enum MessageType
{
  Joke,
  Quote,
  Inspirational,
}

public class MessageGroup
{
  public int MessageGroupId { get; set; }
  public string Name { get; set; }
  public string Category { get; set; }

  public List<BotReply> Replies { get; } = new List<BotReply>();
}

public class Tag
{
  public int TagId { get; set; }
  public string Value { get; set; }

  public int BotReplyId { get; set; }
  public BotReply Reply { get; set; }
}

public class BotReply
{
  public int BotReplyId { get; set; }
  public string Title { get; set; }
  public string Context { get; set; }
  public string Reference { get; set; }
  public List<Tag> Tags { get; set; }

  public int MessageGroupId { get; set; }
  public MessageGroup Group { get; set; }
}
