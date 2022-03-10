﻿using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace bot.Modules;

public sealed class MusicModule : CustomModule<SocketCommandContext>
{
    private readonly LavaNode _lavaNode;

    public MusicModule(LavaNode lavaNode)
    {
        _lavaNode = lavaNode;
    }

    /*
     * add : Add a song to the queue
     * add-playlist : Add a YouTube playlist to the queue
     * clear-queue : Remove every song from the queue
     * join : Make the bot join your voice channel
     * leave : Make the bot leave the voice channel
     * np : Display the current playing track
     * pause : Pause the current playing song
     * play : Start playing from the queue
     * previous : Play the previous track
     * queue : List the songs in the queue
     * replay : Replay the current track
     * resume : Resume playing the current song
     * search : Search for a song
     * seek : Change the current track's position
     * skip : Skip to the next song
     * stop : Stop the current song
     * volume : Change the volume of the player
     * vote-skip : Starts a vote to skip the next song
     */
    [Command("join")]
    public async Task JoinAsync()
    {
        if (_lavaNode.HasPlayer(Context.Guild))
        {
            await ReplyAsync("I'm already connected to a voice channel!");
            return;
        }

        var voiceState = Context.User as IVoiceState;

        if (voiceState?.VoiceChannel == null)
        {
            await ReplyAsync("You must be connected to a voice channel!");
            return;
        }

        try
        {
            await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
        }
        catch (Exception exception)
        {
            await ReplyAsync(exception.Message);
        }
    }

    [Command("play")]
    public async Task PlayAsync([Remainder] string searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            await ReplyAsync("Please provide search terms.");
            return;
        }

        if (!_lavaNode.HasPlayer(Context.Guild))
        {
            await ReplyAsync("I'm not connected to a voice channel.");
            return;
        }

        var searchResponse = await _lavaNode.SearchAsync(SearchType.YouTube, searchQuery);
        if (searchResponse.Status == SearchStatus.LoadFailed ||
            searchResponse.Status == SearchStatus.NoMatches)
        {
            await ReplyAsync($"I wasn't able to find anything for `{searchQuery}`.");
            return;
        }

        var player = _lavaNode.GetPlayer(Context.Guild);

        if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
        {
            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
            {
                foreach (var track in searchResponse.Tracks)
                {
                    player.Queue.Enqueue(track);
                }

                await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
            }
            else
            {
                var track = searchResponse.Tracks.First();
                player.Queue.Enqueue(track);
                await ReplyAsync($"Enqueued: {track.Title}");
            }
        }
        else
        {
            var track = searchResponse.Tracks.First();

            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
            {
                for (var i = 0; i < searchResponse.Tracks.Count; i++)
                {
                    if (i == 0)
                    {
                        await player.PlayAsync(track);
                        await ReplyAsync($"Now Playing: {track.Title}");
                    }
                    else
                    {
                        player.Queue.Enqueue(searchResponse.Tracks.ElementAt(i));
                    }
                }

                await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
            }
            else
            {
                await player.PlayAsync(track);
                await ReplyAsync($"Now Playing: {track.Title}");
            }
        }
    }

    [Command("skip")]
    public async Task Skip()
    {
        var player = await Check();
        if (player == null) return;

        if (player.Queue.Count == 0)
        {
            await ReplyAsync("There are no more songs in the queue!");
            return;
        }

        await player.SkipAsync();
        await ReplyAsync($"Now playing {player.Track.Title} from {player.Track.Author} on {player.Track.Source}");
    }

    [Command("pause")]
    public async Task Pause()
    {
        var player = await Check();
        if (player == null) return;
        if (player.PlayerState == PlayerState.Paused)
        {
            await ReplyAsync("Nothing is playing at the moment");
            return;
        }
        await player.PauseAsync();
        await ReplyAsync($"Paused the music");
    }

    [Command("resume")]
    public async Task Resume()
    {
        var player = await Check();
        if (player == null) return;
        if (player.PlayerState == PlayerState.Playing)
        {
            await ReplyAsync("Music is already playing");
            return;
        }
        await player.ResumeAsync();
        await ReplyAsync($"Resumed playing the music");
    }

    [Command("leave")]
    public async Task Leave()
    {
        var player = await Check();
        if (player == null) return;
        var voiceState = Context.User as IVoiceState;
        await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
        await ReplyAsync("Left voice channel");
    }

    [Command("stop")]
    public async Task Stop()
    {
        var player = await Check();
        if (player == null) return;
        if (player.PlayerState == PlayerState.Stopped)
        {
            await ReplyAsync("There is nothing playing");
            return;
        }
        await player.StopAsync();
        await ReplyAsync("Stopped playing music");
    }

    public async Task<LavaPlayer> Check()
    {
        var voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null)
        {
            await ReplyAsync("You must be connected to a voice channel!");
            return null;
        }

        if (!_lavaNode.HasPlayer(Context.Guild))
        {
            await ReplyAsync("I'm not connected to a voice channel!");
            return null;
        }

        var player = _lavaNode.GetPlayer(Context.Guild);
        if (voiceState.VoiceChannel != player.VoiceChannel)
        {
            await ReplyAsync("You need to be in the same voice channel as me!");
            return null;
        }

        return player;
    }
}
