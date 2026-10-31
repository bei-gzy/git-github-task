namespace App.KeyBoard;

public class KeyBoardEventArgs(ConsoleKeyInfo? key) : EventArgs
{
	public ConsoleKeyInfo? KeyInfo { get; } = key;
}

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
