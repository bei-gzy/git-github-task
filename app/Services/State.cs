namespace App.Services;

public class State
{
	public record Point(int X, int Y);

	public static bool IsGameOver { get; set; } = false;

	public enum Direction
	{
		Up,
		Down,
		Left,
		Right
	}

	public enum GamingState
	{
		GamingStateRunning,
		GamingGameover,
		Exite
	}
}
