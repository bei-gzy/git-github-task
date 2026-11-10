namespace App.Services;

public class State
{
	public record Point(int X, int Y);

	public enum Direction
	{
		Up,
		Down,
		Left,
		Right
	}
}
