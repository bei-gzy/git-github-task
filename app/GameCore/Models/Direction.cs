namespace App.GameCore.Models;

public record Point(int X, int Y);

public enum DirectionType
{
	Up,
	Down,
	Left,
	Right
}
