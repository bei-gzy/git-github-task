using App.Game.Screen;
using App.World;

namespace App.Services;

class FuncSystem//单人模式
{
	public static async Task<bool> GameRunSystemAsync(GamePrepareArgs args)
	{
		var world = args.GameWorld;
		var player = args.Player;
		var tick = TimeSpan.FromMilliseconds(world.TickMilliseconds);
		using var timer = new PeriodicTimer(tick);

		bool died = false;

		Draw(player, world);

		while (!world.IsOver)
		{
			//单人：按键全部交给 Button 事件，和双人同一段代码
			args.PumpInput();

			if (world.QuitRequired) break;

			world.UpdateTime(tick);
			if (world.IsOver) break;

			player.MSnake.Nextdir = args.LocalInput.Direction;
			player.Skills.MoveMode(world, player);
			world.UpdateFood(tick);
			args.Collision.TouchCollision(player.MSnake, player.MScore, world, args.Score);

			if (world.IsOver)
			{
				died = true;
				player.IsAlive = false;
				break;
			}

			Draw(player, world);

			if (!await timer.WaitForNextTickAsync()) break;
		}

		player.RecordSurvivalTime(world.ElapsedTime);

		if (!world.QuitRequired)
			ShowResult(player, world, died);

		return !world.QuitRequired;
	}

	static void Draw(Player player, GameWorld world)
	{
		Console.Clear();
		Console.WriteLine("——————————————Welcome TO Snake Eat Game——————————————");
		Console.WriteLine("法则:WASD控制方向,E加速(冷却5秒),Esc退出游戏");
		Console.WriteLine();

		world.PrintMap();

		double left = Math.Max(0, world.GameDurationSeconds - world.ElapsedTime.TotalSeconds);
		Console.WriteLine($"得分：{player.MScore.MyScore}　长度：{player.MSnake.Snake.Count}　剩余：{left:F0} 秒");
	}

	static void ShowResult(Player player, GameWorld world, bool died) => DefeatSystem.Result(player, world, died);
}
