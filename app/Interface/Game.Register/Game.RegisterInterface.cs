namespace Game.Register;

public class GameRegister
{
	public event Action<Player>? GameRegisterInterface;

	public void Register(string? snakename) => GameRegisterInterface?.Invoke(new Player(snakename));

	public void TouchGameRegister(string? snakename) => Register(snakename);
}

public class GameRegisterSystem()
{
	Action<Player>? mGameRegisterInterface;

	public void DoGameRegisterSystem(GameRegister s, string? snakename)
	{
		mGameRegisterInterface = (player) =>
		{
			player.SetSnakeName(snakename);
		};

		s.GameRegisterInterface += mGameRegisterInterface;
	}

	public void CancelGameRegisterSystem(GameRegister s)
	{
		if (mGameRegisterInterface is not null)
		{
			s.GameRegisterInterface -= mGameRegisterInterface;
		}
	}
}

public class UseGameRegisterSystem()
{
	public static void ToUseGameRegisterSystem(string? sname, Player player, GameRegister gameRegister, GameRegisterSystem gameRegisterSystem)
	{
		gameRegisterSystem.DoGameRegisterSystem(gameRegister, sname);

		gameRegister.TouchGameRegister(sname);

		gameRegisterSystem.CancelGameRegisterSystem(gameRegister);
	}
}
