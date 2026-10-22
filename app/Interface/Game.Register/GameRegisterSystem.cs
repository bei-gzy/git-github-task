namespace App.Game.Register;

public class GameRegisterSystem
{
	static bool IsRegisterCompleted { get; set; } = false;

	public static async Task UseRegister()
	{
		if (PlayerRegisterInterface.PrintPlayerRegisterInterface())
		{
			try
			{
				if (await Register())
				{
					Console.WriteLine("登陆成功！");
				}
			}
			catch { Console.WriteLine("登陆失败！"); }//添加异常
		}
	}

	public static async Task<bool> Register()
	{
		//公告
		var announcement = Announcement.Announce();
		//通知
		var notification = Notification.Notify("登陆送五星皮肤");

		//注册
		bool waitplayername = await Task.Run(PlayerRegisterInterface.PrintPlayerRegisterInterface);

		if (waitplayername)
		{
			IsRegisterCompleted = true;
		}

		Console.Clear();

		return IsRegisterCompleted;
	}
}

//通知
public class Notification
{
	public static async Task Notify(string message)
	{
		await Task.Delay(0);
		Console.WriteLine(message);
	}
}

//公告

public class Announcement
{
	static readonly string[] Announcements =
	[
		"游戏首天公测",
		"欢迎来到贪吃蛇游戏！",
		"注册成功！",
	];

	public static async Task Announce()
	{
		foreach (string message in Announcements)
		{
			await Task.Delay(5);
			Console.WriteLine(message);
		}
	}
}

