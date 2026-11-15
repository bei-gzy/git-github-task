namespace App.GameCore;

public sealed record PlayerState(
	string Id,
	string Name,
	SnakeState Snake,
	int Score,
	double SurvivalTime
)
{
	public string Id { get; init; } = Id;
	public string? Name { get; init; } = Name;
	public SnakeState Snake { get; init; } = Snake;
	public int Score { get; init; } = Score;
	//Skills Skills;
	public double SurvivalTime { get; init; } = SurvivalTime;
}

