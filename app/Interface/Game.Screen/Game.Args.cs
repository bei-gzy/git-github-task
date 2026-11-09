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

	public KeyBoardController LocalInput { get; }

	public KeyBoardController? GuestInput { get; private set; }

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

		LocalInput = new KeyBoardController(Button, Player, KeyLayout.Wasd, ConsoleKey.E);
		quit = new QuitController(Button, GameWorld);
		CollisionController = new CollisionController(Collision);
		Session = session;
	}

	public void BeginRound()
	{
		GameWorld.Restart();
		Player = GameWorld.AddPlayer(local, isLocal: true);
		LocalInput.Begin(State.Direction.Down);
		quit.Begin();
		CollisionController.Subscribe();
		Session?.PublishSnapShot(GameWorld);
	}

	public void EndRound()
	{
		Session?.StopSnapShot();
		LocalInput.End();
		GuestInput?.End();
		GuestInput = null;
		quit.End();
		CollisionController.Unsubscribe();
	}

	public KeyBoardController EnableGuestInput(Player guest, State.Direction dir)
	{
		GuestInput?.End();
		GuestInput = new KeyBoardController(Button, guest, KeyLayout.Arrows, ConsoleKey.NumPad0, ConsoleKey.D0);
		GuestInput.Begin(dir);
		return GuestInput;
	}
}
