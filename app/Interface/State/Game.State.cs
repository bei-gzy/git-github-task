namespace App.Interface.State;

public class GameState
{
	public GameOnAndOffState GameRunningState { get; set; } = GameOnAndOffState.GameOn;

	public enum InterfaceState
	{
		ChoosingMode,
		SinglePlayerMode,
		MultiPlayerMode,
		ExitMode,
	}

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

