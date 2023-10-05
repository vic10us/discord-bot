using v10.Games.EightBall.Models;

namespace v10.Games.EightBall;

public class EightBallService : IEightBallService
{
    private static readonly EightBallResponse[] _responses = new[]
    {
        // Affirmative
        new EightBallResponse(1,  "It is certain",       AnswerType.Affirmative),
        new EightBallResponse(2,  "It is decidedly so",  AnswerType.Affirmative),
        new EightBallResponse(3,  "Without a doubt",     AnswerType.Affirmative),
        new EightBallResponse(4,  "Yes definitely",      AnswerType.Affirmative),
        new EightBallResponse(5,  "You may rely on it",  AnswerType.Affirmative),
        new EightBallResponse(6,  "As I see it, yes",    AnswerType.Affirmative),
        new EightBallResponse(7,  "Most likely",         AnswerType.Affirmative),
        new EightBallResponse(8,  "Outlook good",        AnswerType.Affirmative),
        new EightBallResponse(9,  "Yes",                 AnswerType.Affirmative),
        new EightBallResponse(10, "Signs point to yes",  AnswerType.Affirmative),

        // Non-Committal
        new EightBallResponse(11, "Reply hazy, try again",     AnswerType.NonCommittal),
        new EightBallResponse(12, "Ask again later",           AnswerType.NonCommittal),
        new EightBallResponse(13, "Better not tell you now",   AnswerType.NonCommittal),
        new EightBallResponse(14, "Cannot predict now",        AnswerType.NonCommittal),
        new EightBallResponse(15, "Concentrate and ask again", AnswerType.NonCommittal),

        // Negative
        new EightBallResponse(16, "Don't count on it",   AnswerType.Negative),
        new EightBallResponse(17, "My reply is no",      AnswerType.Negative),
        new EightBallResponse(18, "My sources say no",   AnswerType.Negative),
        new EightBallResponse(19, "Outlook not so good", AnswerType.Negative),
        new EightBallResponse(20, "Very doubtful",       AnswerType.Negative),
    };

    public EightBallService()
    {
    }

    public EightBallResponse GetRandomResponse()
    {
        var r = new Random();
        var index = r.Next(0, _responses.Length);
        return _responses[index];
    }
}
