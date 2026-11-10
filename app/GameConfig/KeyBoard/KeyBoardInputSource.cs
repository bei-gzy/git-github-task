using App.Services;

namespace App.KeyBoard;

public enum KeyLayout
{
	Wasd,
	Arrows
}

public sealed class KeyboardInputSource(
	Button button, KeyLayout layout, ConsoleKey dashKey, State.Direction start,
	ConsoleKey? altDashKey = null) : IInputSource
{
	State.Direction dir = start;
	bool isDash;

	public void Begin()
	{
		button.Clicked -= OnClicked;
		button.Clicked += OnClicked;
	}

	public void End() => button.Clicked -= OnClicked;

	public PlayerIntent Poll()
	{
		bool dash = isDash;
		isDash = false;
		return new PlayerIntent(dir, dash);
	}

	void OnClicked(object? sender, KeyBoardEventArgs e)
	{
		var key = e.KeyInfo?.Key;
		if (key is null) return;

		if (key == dashKey || (altDashKey is { } alt && key == alt))
		{
			isDash = true;
			return;
		}

		dir = key switch
		{
			ConsoleKey.W when layout is KeyLayout.Wasd && dir is not State.Direction.Down => State.Direction.Up,
			ConsoleKey.S when layout is KeyLayout.Wasd && dir is not State.Direction.Up => State.Direction.Down,
			ConsoleKey.A when layout is KeyLayout.Wasd && dir is not State.Direction.Right => State.Direction.Left,
			ConsoleKey.D when layout is KeyLayout.Wasd && dir is not State.Direction.Left => State.Direction.Right,

			ConsoleKey.UpArrow when layout is KeyLayout.Arrows && dir is not State.Direction.Down => State.Direction.Up,
			ConsoleKey.DownArrow when layout is KeyLayout.Arrows && dir is not State.Direction.Up => State.Direction.Down,
			ConsoleKey.LeftArrow when layout is KeyLayout.Arrows && dir is not State.Direction.Right => State.Direction.Left,
			ConsoleKey.RightArrow when layout is KeyLayout.Arrows && dir is not State.Direction.Left => State.Direction.Right,

			_ => dir
		};
	}
}
