using App.World;

namespace App.KeyBoard;

/// <summary>Esc 退出：单人和双人共用一个订阅，别让每个方向控制器各判一次</summary>
class QuitController(Button button, GameWorld world)
{
	public void Begin()
	{
		button.Clicked -= OnClicked;
		button.Clicked += OnClicked;
	}

	public void End() => button.Clicked -= OnClicked;

	void OnClicked(object? sender, KeyBoardEventArgs e)
	{
		if (e.KeyInfo?.Key is ConsoleKey.Escape)
			world.QuitGame();
	}
}
