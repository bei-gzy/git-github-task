using App.World;

namespace App.KeyBoard;

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
