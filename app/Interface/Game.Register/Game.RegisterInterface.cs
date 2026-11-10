using App.World;

namespace App.Interface;

class PlayerRegisterInterface
{
	internal static List<Player> PlayerLists = [];//注册游戏的玩家名单

	internal static Player? CurrentPlayer { get; private set; }

	static bool IsSetPlayerName { get; set; } = false;

	public static bool PrintPlayerRegisterInterface()
	{
		Console.WriteLine("欢迎来到贪吃蛇游戏！");

		while (!IsSetPlayerName)
		{
			Console.WriteLine("请为你的贪吃蛇设置一个名字： ");
			string? playername = Console.ReadLine()?.Trim();

			if (string.IsNullOrWhiteSpace(playername))
			{
				Console.WriteLine("请输入有效的名字！");
				continue;
			}

			if (PlayerLists.Any(p => p.Name == playername))
			{
				Console.WriteLine("名字" + playername + "已存在，请重新输入！");
				continue;
			}

			Console.WriteLine("已将你的名字已设置为：" + playername);
			var a = new Player(Guid.NewGuid().ToString("N")[..8]);//随机id--ai牛
			a.SetName(playername);
			PlayerLists.Add(a);
			CurrentPlayer = a;
			break;
		}

		return IsSetPlayerName = true;
	}
}
