using App.GameScore;
using App.Models;
using App.Services;

namespace App.World;

public class Player(string id)
{
	public string Id { get; } = id;

	public string? Name { get; private set; }

	public bool IsAlive { get; set; } = true;

	public bool IsTouchButton { get; set; }

	public SnakeHead MSnake { get; private set; } = new(10, 15, State.Direction.Down, 3);

	public PlayerScore MScore { get; private set; } = new();

	internal Skills Skills { get; } = new();

	public double SurvivalTime { get; private set; }

	public void SetName(string playername) => Name = playername;

	public void Rename(string? name) => Name = name;

	public void RecordSurvivalTime(TimeSpan elapsed) => SurvivalTime = elapsed.TotalSeconds;

	public void ResetForNewGame() => ResetForNewGame(10, 15, State.Direction.Down);

	public void ResetForNewGame(int x, int y, State.Direction dir)
	{
		MSnake = new SnakeHead(x, y, dir, 3);
		MScore = new();
		SurvivalTime = 0;
		IsAlive = true;
		IsTouchButton = false;
		Skills.Reset();
	}
}
