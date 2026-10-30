namespace App.Game.Register;

class GameRegisterSystem : PlayerRegisterInterface
{
	public static async Task UseRegister()
	{
		if (await Register())
		{
			Console.WriteLine("登陆成功！");
			await Task.Delay(200);//加载游戏数据
			return;
		}

		Console.WriteLine("登陆失败！");
	}

	public static async Task<bool> Register()
	{
		//注册
		bool waitplayername = await Task.Run(PrintPlayerRegisterInterface);

		if (waitplayername)
		{
			Console.Clear();
			return true;
		}

		return false;
	}
}
