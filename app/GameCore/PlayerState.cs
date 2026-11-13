
namespace App.GameCore;

public sealed record PlayerState(
	string Id,
	string Name,
	SnakeState Snake
)
{
	public string Id { get; init; } = Id;
	public string? Name { get; init; } = Name;
	public bool IsTouchButton { get; init; } = false;
	public SnakeState Snake { get; init; } = Snake;
	public int Score { get; init; } = 0;
	//Skills Skills;
	public double SurvivalTime { get; init; } = 0;
}

// public Snake MSnake { get; private set; } = new(10, 15, Direction.Down, 3);

// public void SetName(string playername) => Name = playername;

// public void Rename(string? name) => Name = name;

// public void RecordSurvivalTime(TimeSpan elapsed) => SurvivalTime = elapsed.TotalSeconds;

// public void ResetForNewGame() => ResetForNewGame(10, 15, Direction.Down);

// public void ResetForNewGame(int x, int y, Direction dir)
// {
// 	MSnake = new Snake(x, y, dir, 3);
// 	MScore = new();
// 	SurvivalTime = 0;
// 	IsAlive = true;
// 	IsTouchButton = false;
// 	Skills.Reset();
// }
