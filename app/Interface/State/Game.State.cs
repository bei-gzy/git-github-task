namespace App.GameStates;

public class GameState
{
	public GameOnAndOffState GameRunningState { get; set; } = GameOnAndOffState.GameOn;

	public enum HomeScreenState
	{
		Choosegamemode,
		//Setmenu,
		Exitgame,
	}

	public enum GameOnAndOffState
	{
		GameOn,
		GameOff,
	}
}
