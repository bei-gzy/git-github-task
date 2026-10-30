namespace App.KeyBoard;

public class KeyBoardEventArgs(ConsoleKeyInfo? key) : EventArgs
{
	public ConsoleKeyInfo? KeyInfo { get; } = key;
}

//按键分发：读键的地方负责 TouchClick，订阅者（KeyBoardController × N / QuitController）收事件
public class Button()
{
	public event EventHandler<KeyBoardEventArgs>? Clicked;

	public void TouchClick(ConsoleKeyInfo? keyInfo)
	{
		var clickinfo = new KeyBoardEventArgs(keyInfo);

		OnClicked(clickinfo);
	}

	//触发
	protected virtual void OnClicked(KeyBoardEventArgs e) => Clicked?.Invoke(this, e);
}