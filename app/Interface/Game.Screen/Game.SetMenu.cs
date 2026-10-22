using App.GameTime;
using App.Interface.State;
namespace App.Game.Screen;

class GameSetMenu : HomeScreen
{
	public static async Task<GameState.HomeScreenState> SetMenu(GamePrepareArgs args)
	{
		while (true)
		{
			Console.Clear();
			PrintOptions();
			var tkey = ReadKey().Key;
			if (tkey is ConsoleKey.D4 && tkey is ConsoleKey.D1 && tkey is ConsoleKey.D2 && tkey is ConsoleKey.D3)
			{
				if (await await Task.WhenAny(ChangeMapSize(), ChangeInitSpeed(), ChangeTime(args)))
				{
					break;
				}
			}

			Console.WriteLine("输入有误，请重新输入！");
		}

		return hstate;
	}

	static void PrintOptions()
	{
		string[] choice =
		[
			"1、设置地图大小",
			"2、设置初始速度",
			"3、设置游戏时间",
			"4、退出设置菜单"
		];

		foreach (string e in choice)
		{
			Console.WriteLine(e);
		}
	}

	static Task<bool> ChangeMapSize() => Task.FromResult(true);

	static Task<bool> ChangeInitSpeed() => Task.FromResult(true);

	static Task<bool> ChangeTime(GamePrepareArgs args)
	{
		SetTime.ChangeTime(args);

		return Task.FromResult(true);
	}
}
