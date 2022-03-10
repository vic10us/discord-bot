namespace v10.Services.DadJokes.Models;

public class DadJoke : IDadJoke
{
    public string Id { get; set; }
    public string Joke { get; set; }
    public int Status { get; set; }
}
