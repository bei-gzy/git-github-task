using App.World;

namespace App.Services;

class DefeatSystem
{
	public static void Result(Player player, GameWorld world, bool died)
	{
		Console.Clear();
		Console.ResetColor();

		Console.WriteLine(died ? "Gaming Over!" : "时间到！");
		Console.WriteLine("得分: " + player.MScore.MyScore);
		Console.WriteLine("长度: " + player.MSnake.Snake.Count);
		Console.WriteLine($"存活: {world.ElapsedTime.TotalSeconds:F1} 秒");
		Console.WriteLine();
		Console.WriteLine("按任意键返回主菜单…");

		_ = Console.ReadKey(intercept: true);
	}
	//是否复活--待开发
}
