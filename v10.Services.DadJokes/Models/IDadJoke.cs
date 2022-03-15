namespace v10.Services.DadJokes.Models;

public interface IDadJoke
{
  string Id { get; set; }
  string Joke { get; set; }
  int Status { get; set; }
}
