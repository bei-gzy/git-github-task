// using App.Game.Screen;
// using App.World;

// namespace App.Services;

// class FuncSystem//单人模式
// {
// 	public static async Task<bool> GameRunSystemAsync(GamePrepareArgs args)
// 	{
// 		var world = args.GameWorld;
// 		var player = args.Player;
// 		var tick = TimeSpan.FromMilliseconds(world.TickMilliseconds);
// 		using var timer = new PeriodicTimer(tick);

// 		while (!world.IsOver)
// 		{
// 			while (Console.KeyAvailable)
// 				args.Button.TouchClick(Console.ReadKey(intercept: true));

// 			if (world.QuitRequired) break;

// 			world.UpdateTime(tick);
// 			player.MSnake.Nextdir = args.LocalInput.Direction;
// 			player.Skills.MoveMode(world, player);
// 			world.UpdateFood(tick);
// 			Collision(args);

// 			if (world.IsOver)
// 			{
// 				player.IsAlive = false;
// 				break;
// 			}

// 			Draw(world);

// 			if (!await timer.WaitForNextTickAsync()) break;
// 		}

// 		player.RecordSurvivalTime(world.ElapsedTime);

// 		if (!world.QuitRequired)
// 			EndSystem(player, world);

// 		return !world.QuitRequired;
// 	}

// 	static void Draw(GameWorld world) => MapPrinter.Draw(world.Snapshot(),
// 		"——————————————Welcome TO Snake Eat Game——————————————",
// 		"法则:WASD控制方向,E加速(冷却5秒),Esc退出游戏"
// 		);

// 	static void Collision(GamePrepareArgs args)
// 	{
// 		if (args.Player.IsAlive)
// 		{
// 			foreach (var p in args.GameWorld.Players)
// 			{
// 				args.Collision.TouchCollision(p, args.GameWorld);
// 			}
// 		}
// 	}

// 	static void EndSystem(Player player, GameWorld world) => DefeatSystem.SingleEndSystem(player, world);
// }
