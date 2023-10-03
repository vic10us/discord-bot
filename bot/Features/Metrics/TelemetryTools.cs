using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace bot.Features.Metrics;

public static class TelemetryTools
{
    public static ActivitySource activitySource = new ActivitySource("DiscordBotActivitySource");
    public static Meter s_meter = new("DiscordBotMetrics", "1.0.0");
    public static Counter<int> s_botCommandsHandled = s_meter.CreateCounter<int>(name: "bot_commands_handled",
                                                                unit: "Commands",
                                                                description: "The number of commands handled");
    public static Histogram<int> s_orderProcessingTimeMs = s_meter.CreateHistogram<int>("order_processing_time");
    public static int s_coatsSold;
    public static int s_ordersPending;

    public static void BotCommandHandled(string command)
    {
        s_botCommandsHandled.Add(1, new KeyValuePair<string, object>("command", command));
    }

    public static void Init()
    {
        s_botCommandsHandled.Add(1);
        // s_botCommandsHandled.Add(-1);
        // do nothing. ;)
    }

}
