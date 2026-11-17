using App.GameCore.Models;

namespace App.GameCore;

public sealed record PlayerState(
	string Id,
	string Name,
	SnakeState Snake,
	int Score = 0,
	double SurvivalTime = 0
)
{
	public static PlayerState Create(string id, string name, Point head, Direction direction, int length)
		=> new(id, name, SnakeState.Create(head, direction, length));
}

