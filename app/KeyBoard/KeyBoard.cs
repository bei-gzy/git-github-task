using App.Services;
// using App.Models;

namespace App.KeyBoard;

public class KeyBoardEventArgs(ConsoleKeyInfo? key) : EventArgs
{
	public ConsoleKeyInfo? KeyInfo { get; } = key;
}

public class Button()
{
	//声明事件
	public event EventHandler<KeyBoardEventArgs>? Clicked;

	public void TouchClick(ConsoleKeyInfo? keyInfo)
	{
		var clickinfo = new KeyBoardEventArgs(keyInfo);

		OnClicked(clickinfo);
	}

	//触发
	protected virtual void OnClicked(KeyBoardEventArgs e) => Clicked?.Invoke(this, e);
}

public class KeyBoardController
{
	readonly Button? button;

	public State.Direction Direction { get; private set; }

	public State.GamingState ExitState { get; private set; }

	public KeyBoardController(Button button, State.Direction dir)
	{
		this.button = button;
		Direction = dir;
		this.button.Clicked += OnClicked;
	}

	void OnClicked(object? sender, KeyBoardEventArgs e)
	{
		if (e.KeyInfo is { Key: ConsoleKey.Escape })//e.keyInfo != null && e.KeyInfo.Value.Key == ConsoleKey.Escape)
		{
			ExitState = State.GamingState.Exite;
		}

		var nextDirection = e.KeyInfo?.Key switch
		{
			ConsoleKey.W when Direction != State.Direction.Down => State.Direction.Up,
			ConsoleKey.S when Direction != State.Direction.Up => State.Direction.Down,
			ConsoleKey.A when Direction != State.Direction.Right => State.Direction.Left,
			ConsoleKey.D when Direction != State.Direction.Left => State.Direction.Right,
			_ => Direction
		};

		Direction = nextDirection;
	}

	public void CancleClicked() => button?.Clicked -= OnClicked;
}

// public class DelegateVariable(Button button, State.Direction dir, State.GamingState state)
// {
// 	EventHandler<KeyBoardEventArgs>? mbutton;

// 	public void DoClick()
// 	{
// 		mbutton = (sender, e) =>
// 		{
// 			if (e.KeyInfo.Key == ConsoleKey.Escape)
// 			{
// 				state = State.GamingState.Exite;
// 			}

// 			var nextdir = e.KeyInfo.Key switch
// 			{
// 				ConsoleKey.W when dir != State.Direction.Down => State.Direction.Up,
// 				ConsoleKey.S when dir != State.Direction.Up => State.Direction.Down,
// 				ConsoleKey.A when dir != State.Direction.Right => State.Direction.Left,
// 				ConsoleKey.D when dir != State.Direction.Left => State.Direction.Right,
// 				_ => dir
// 			};

// 			dir = nextdir;

// 		};

// 		button.Clicked += mbutton;
// 	}

// 	public void CancleDoClick()
// 	{
// 		if (mbutton != null)
// 		{
// 			button.Clicked -= mbutton;
// 			mbutton = null;
// 		}
// 	}
// }
