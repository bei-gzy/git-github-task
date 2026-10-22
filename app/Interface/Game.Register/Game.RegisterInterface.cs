using Game.Register;

namespace App.Game.Register;

public class PlayerRegisterInterface
{
	static readonly List<Player> PlayerNames = [];

	static bool IsSetPlaeyerName { get; set; } = false;

	public static bool PrintPlayerRegisterInterface()
	{
		Console.WriteLine("欢迎来到贪吃蛇游戏！");
		while (!IsSetPlaeyerName)
		{
			Console.WriteLine("请为你的贪吃蛇设置一个名字： ");
			string? playername = Console.ReadLine();
			foreach (var player in PlayerNames)
			{
				if (playername == player.Name)
				{
					Console.WriteLine("名字" + playername + "已存在，请重新输入！");
					break;
				}
			}

			if (playername is not null)
			{
				Console.WriteLine("已将你的名字已设置为：" + playername);
				PlayerNames.Add(new Player(playername));
				//IsSetPlaeyerName = true;
				break;
			}

			Console.WriteLine("请输入有效的名字！");
		}

		return IsSetPlaeyerName = true;
	}
}
