namespace App.GameCore.Models;

public record Point(int X, int Y);

public enum Direction
{
	Up,
	Down,
	Left,
	Right
}
