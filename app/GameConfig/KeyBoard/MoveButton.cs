using App.Services;
using App.World;

namespace App.KeyBoard;

public enum KeyLayout
{
	Wasd,
	Arrows
}

public class KeyBoardController(Button button, Player player, KeyLayout layout, ConsoleKey dashKey, ConsoleKey? altDashKey = null)
{
	public State.Direction Direction { get; private set; }

	public void Begin(State.Direction direction)
	{
		Direction = direction;
		button.Clicked -= OnClicked;
		button.Clicked += OnClicked;
	}

	public void End() => button.Clicked -= OnClicked;

	void OnClicked(object? sender, KeyBoardEventArgs e)
	{
		var key = e.KeyInfo?.Key;
		if (key is null) return;

		if (key == dashKey || (altDashKey is { } alt && key == alt))//技能键和移动建分开处理
		{
			player.IsTouchButton = true;
			return;
		}

		Direction = key switch
		{
			ConsoleKey.W when layout is KeyLayout.Wasd && Direction is not State.Direction.Down => State.Direction.Up,
			ConsoleKey.S when layout is KeyLayout.Wasd && Direction is not State.Direction.Up => State.Direction.Down,
			ConsoleKey.A when layout is KeyLayout.Wasd && Direction is not State.Direction.Right => State.Direction.Left,
			ConsoleKey.D when layout is KeyLayout.Wasd && Direction is not State.Direction.Left => State.Direction.Right,

			ConsoleKey.UpArrow when layout is KeyLayout.Arrows && Direction is not State.Direction.Down => State.Direction.Up,
			ConsoleKey.DownArrow when layout is KeyLayout.Arrows && Direction is not State.Direction.Up => State.Direction.Down,
			ConsoleKey.LeftArrow when layout is KeyLayout.Arrows && Direction is not State.Direction.Right => State.Direction.Left,
			ConsoleKey.RightArrow when layout is KeyLayout.Arrows && Direction is not State.Direction.Left => State.Direction.Right,

			_ => Direction
		};
	}
}
