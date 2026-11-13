namespace App.Application.UI;

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
