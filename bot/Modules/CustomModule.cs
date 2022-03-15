using Discord;
using Discord.Commands;
using System.IO;
using System.Threading.Tasks;

namespace bot.Modules;

public abstract class CustomModule<T> : ModuleBase<T> where T : class, ICommandContext
{
  protected virtual async Task<IUserMessage> ReplyAsyncEx(string message = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null, ISticker[] stickers = null, Embed[] embeds = null)
  {
    return await Context.Channel.SendMessageAsync(message, isTTS, embed, options, allowedMentions, messageReference, components, stickers, embeds).ConfigureAwait(false);
  }

  protected virtual async Task SendImageEmbed(Stream fileStream, string title, string filename, Color color)
  {
    fileStream.Seek(0, SeekOrigin.Begin);
    var embed = new EmbedBuilder()
        .WithTitle(title)
        .WithImageUrl($"attachment://{filename}")
        .WithColor(color)
        .Build();
    await Context.Channel.SendFileAsync(stream: fileStream, filename: filename, embed: embed);
  }
}
