using App.Game.Screen;
using App.World;

namespace App.Services;

//本机双人：只负责“组装”（建玩家2、绑第二套按键），流程交给 Round
class LocalDoubleSystem
{
	public static async Task<bool> GameRunSystemAsync(GamePrepareArgs args)
	{
		var world = args.GameWorld;
		var p1 = args.Player;

		var p2 = world.FindPlayer("guest") ?? new Player("guest");
		if (world.FindPlayer(p2.Id) is null)
			p2 = world.AddPlayer(p2);

		var room = new RoomPlayerLists();
		if (!room.AddPlayer(p1) || !room.AddPlayer(p2))
		{
			Console.WriteLine("房间已满，无法开始对局");
			_ = Console.ReadKey(intercept: true);
			return false;
		}

		p1.SetName("玩家1");
		p2.SetName("玩家2");
		p1.ResetForNewGame(10, world.Height / 3, State.Direction.Down);
		p2.ResetForNewGame(world.Width - 11, world.Height * 2 / 3, State.Direction.Up);

		var p2Input = args.EnableGuestInput(p2, State.Direction.Up);

		await Round.RunAsync(args, new RoundContext(
			"玩家1: W A S D / E 加速   玩家2: ↑ ↓ ← → / 0 加速   Esc 退出",
			"————— 本机双人对战 —————",
			[args.LocalInput, p2Input],
			new DoubleRules(),
			args.Renderer));

		p1.RecordSurvivalTime(world.ElapsedTime);
		p2.RecordSurvivalTime(world.ElapsedTime);

		return !world.QuitRequired;
	}
}
