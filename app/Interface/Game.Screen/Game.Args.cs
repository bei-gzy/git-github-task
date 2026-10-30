using App.Game.Register;
using App.Services;
using App.GameScore;
using App.GameCollision;
using App.KeyBoard;
using App.Config;
using App.World;

namespace App.Game.Screen;

class GamePrepareArgs
{
	//登录时创建的账号，每局都用同一个对象
	readonly Player account;

	//Esc 退出（单人和双人共用）
	readonly QuitController quit;

	public GameWorld GameWorld { get; }

	public GameSettings Settings { get; }

	//本局本地玩家
	public Player Player { get; private set; }

	//计分
	public Score Score { get; } = new Score();

	//按键事件源：单人和双人都往它里面灌按键
	public Button Button { get; } = new();

	/// <summary>玩家1（单人模式也只有这一套）：W A S D + E</summary>
	public KeyBoardController LocalInput { get; }

	/// <summary>玩家2：↑ ↓ ← → + 小键盘 0，仅双人模式存在</summary>
	public KeyBoardController? GuestInput { get; private set; }

	//碰撞检测
	public Collision Collision { get; } = new();

	public CollisionController CollisionController { get; }

	public GamePrepareArgs(GameSettings gameSettings)
	{
		Settings = gameSettings;
		account = PlayerRegisterInterface.CurrentPlayer ?? new Player("local");
		GameWorld = new GameWorld(gameSettings);
		Player = account;

		LocalInput = new KeyBoardController(Button, Player, KeyLayout.Wasd, ConsoleKey.E);
		quit = new QuitController(Button, GameWorld);
		CollisionController = new CollisionController(Collision);
	}

	/// <summary>开一局：清掉上一局残留的玩家、复位计时、重新挂事件</summary>
	public void BeginRound()
	{
		GameWorld.ResetPlayers();
		Player = GameWorld.AddPlayer(account, isLocal: true);
		GameWorld.Restart();

		CollisionController.EndGameOnCollision = true;
		LocalInput.Begin(State.Direction.Down);
		quit.Begin();
		CollisionController.Subscribe();
	}

	/// <summary>一局结束：把事件摘掉，避免下一局重复触发</summary>
	public void EndRound()
	{
		LocalInput.End();
		GuestInput?.End();
		GuestInput = null;
		quit.End();
		CollisionController.Unsubscribe();
	}

	/// <summary>把这一帧攒下的按键全部喂给 Button 事件——单人和双人用的是同一段</summary>
	public void PumpInput()
	{
		while (Console.KeyAvailable)
			Button.TouchClick(Console.ReadKey(intercept: true));
	}

	/// <summary>双人开局时给玩家2 挂一套方向键</summary>
	public KeyBoardController EnableGuestInput(Player guest, State.Direction dir)
	{
		GuestInput?.End();
		GuestInput = new KeyBoardController(Button, guest, KeyLayout.Arrows, ConsoleKey.NumPad0, ConsoleKey.D0);
		GuestInput.Begin(dir);
		return GuestInput;
	}
}
