using App.World;

namespace App.Services;

public class DefeatSystem
{
	//单人
	public static void SingleEndSystem(Player player, GameWorld world)
	{
		Console.Clear();
		Console.ResetColor();

		Console.WriteLine(player.IsAlive ? "Gaming Over!" : "时间到！");
		Console.WriteLine("得分: " + player.MScore.MyScore);
		Console.WriteLine("长度: " + player.MSnake.Snake.Count);
		Console.WriteLine($"存活: {world.ElapsedTime.TotalSeconds:F1} 秒");
		Console.WriteLine();
		Console.WriteLine("按任意键返回主菜单…");

		_ = Console.ReadKey(intercept: true);
	}

	//双人
	public static void DoubleEndSystem(Player p1, Player p2, GameWorld world)
	{
		var winner = Winner(p1, p2);

		Console.Clear();
		Console.WriteLine("————— 对局结束 —————");

		bool timeUp = world.ElapsedTime.TotalSeconds >= world.GameDurationSeconds;
		Console.WriteLine(timeUp ? "时间到" : winner is null ? "平局" : $"{winner.Name} 获胜");
		Console.WriteLine($"{p1.Name}: {p1.MScore.MyScore} 分 长度 {p1.MSnake.Snake.Count} {(p1.IsAlive ? "存活" : "死亡")}");
		Console.WriteLine($"{p2.Name}: {p2.MScore.MyScore} 分 长度 {p2.MSnake.Snake.Count} {(p2.IsAlive ? "存活" : "死亡")}");
		Console.WriteLine($"用时 {world.ElapsedTime.TotalSeconds:F1} 秒");
		Console.WriteLine();
		Console.WriteLine("按任意键返回模式菜单…");
		_ = Console.ReadKey(intercept: true);
	}

	public static Player? Winner(Player p1, Player p2) => p1.IsAlive != p2.IsAlive
			? p1.IsAlive ? p1 : p2
			: p1.MSnake.Snake.Count != p2.MSnake.Snake.Count
			? p1.MSnake.Snake.Count > p2.MSnake.Snake.Count ? p1 : p2
			: p1.MScore.MyScore != p2.MScore.MyScore ? p1.MScore.MyScore > p2.MScore.MyScore ? p1 : p2 : null;
}
