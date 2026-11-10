using App.Game.Screen;
using App.World;

namespace App.Services;

//本机双人：只负责“组装”（建玩家2、绑第二套按键），流程交给 Round
class LocalDoubleSystem
{
	public static async Task GameRunSystemAsync(GamePrepareArgs args, string title = "————— 本机双人对战 —————")
	{
		var world = args.GameWorld;
		var p1 = args.Player;

		var p2 = world.FindPlayer("guest") ?? new Player("guest");
		// 查找并复用 guest 玩家；若不存在则创建，随后确保该玩家已加入世界。
		if (world.FindPlayer(p2.Id) is null)
			p2 = world.AddPlayer(p2);

		var room = new RoomPlayerLists();
		if (!room.AddPlayer(p1) || !room.AddPlayer(p2))
		{
			Console.WriteLine("房间已满，无法开始对局");
			_ = Console.ReadKey(intercept: true);
			return;
		}

		p1.ResetForNewGame(10, world.Height / 3, State.Direction.Down);
		p2.ResetForNewGame(world.Width - 11, world.Height * 2 / 3, State.Direction.Up);

		var p2Input = args.EnableGuestInput(State.Direction.Up);

		await Round.RunAsync(args, new RoundContext(
			"玩家1: W A S D / E 加速   玩家2: ↑ ↓ ← → / 0 加速   Esc 退出",
			title,
			[args.LocalInput, p2Input],
			new DoubleRules(),
			args.Renderer));

		p1.RecordSurvivalTime(world.ElapsedTime);
		p2.RecordSurvivalTime(world.ElapsedTime);

		return;
	}
}
