using App.Game.Register;
using App.Services;
using App.GameCollision;
using App.KeyBoard;
using App.Config;
using App.World;
using App.NetWork;

namespace App.Game.Screen;

public class GamePrepareArgs
{
	//登录时创建的账号，每局都用同一个对象
	readonly Player local;

	readonly QuitController quit;

	public GameWorld GameWorld { get; }

	//本局本地玩家
	public Player Player { get; private set; }

	public Button Button { get; } = new();

	public KeyboardInputSource LocalInput { get; }

	public RemoteInputSource? GuestInput { get; private set; }

	//碰撞检测
	public Collision Collision { get; } = new();

	internal CollisionController CollisionController { get; }

	//网络层调用中枢
	public NetSession? Session { get; }

	//打印
	public IRenderer Renderer { get; }
	//结算
	public IRoundResultView ResultView { get; }

	public GamePrepareArgs(GameSettings gameSettings, NetSession? session, IRenderer renderer, IRoundResultView resultView)
	{
		Renderer = renderer;
		ResultView = resultView;
		local = PlayerRegisterInterface.CurrentPlayer ?? new Player("local");
		GameWorld = new GameWorld(gameSettings);
		Player = local;

		LocalInput = new KeyboardInputSource(Button, KeyLayout.Wasd, ConsoleKey.E, State.Direction.Down);
		quit = new QuitController(Button, GameWorld);
		CollisionController = new CollisionController(Collision);
		Session = session;
	}

	public void BeginRound()
	{
		GameWorld.Restart();
		Player = GameWorld.AddPlayer(local, isLocal: true);
		LocalInput.Begin();
		quit.Begin();
		CollisionController.Subscribe();
		Session?.PublishSnapShot(GameWorld);
	}

	public void EndRound()
	{
		Session?.StopSnapShot();
		LocalInput.End();
		GuestInput = null;
		quit.End();
		CollisionController.Unsubscribe();
	}

	public void PumpInput()
	{
		if (Session is null) return;
		while (Session.PumpInput(out var m))
		{
			Session.NextClentTick();
			GuestInput?.ChangeDirection(m);
		}
	}

	public RemoteInputSource EnableGuestInput(State.Direction old)
	{
		GuestInput = new RemoteInputSource(old);
		return GuestInput;
	}
}
