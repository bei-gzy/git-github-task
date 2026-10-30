using App.Services;
using App.World;

namespace App.KeyBoard;

/// <summary>按键布局：单人和双人的差别只在这里，事件流程完全共用</summary>
public enum KeyLayout
{
	Wasd,
	Arrows
}

/// <summary>一个玩家一套：吃 Button 的 Clicked 事件，只认自己那套键</summary>
public class KeyBoardController(Button button, Player player, KeyLayout layout, ConsoleKey dashKey, ConsoleKey? altDashKey = null)
{
	public State.Direction Direction { get; private set; }

	/// <summary>每局开始时复位方向并重新挂订阅（上一局结束时会解绑）</summary>
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

		//技能键（双人玩家2 兼容小键盘 0 和主键盘 0）
		if (key == dashKey || (altDashKey is { } alt && key == alt))
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
