using App.GameCore;
using App.InputAndOutput;
using App.Application.GameRunner;
using App.Infrastructure;
using App.Infrastructure.Mqtt;

namespace App.Application.UI;

public sealed class MenuController(IRenderer renderer, IRoundResultView resultView, Random random, NetOptions netOptions)
{
	readonly IRenderer renderer = renderer;
	readonly IRoundResultView resultView = resultView;
	readonly Random random = random;
	readonly NetOptions netOptions = netOptions;

	// ───── 可配置项（设置菜单修改）─────
	int width = 25;
	int height = 30;
	int tickMs = 200;
	int durationSeconds = 60;
	readonly int foodNum = 3;

	// ───── 本机玩家身份 ─────
	string localId = "";
	string localName = "";

	// 总帧数 = 时长(秒) * 1000 / 每帧(毫秒)
	int MaxTick => Math.Max(1, durationSeconds * 1000 / tickMs);

	public async Task RunAsync(CancellationToken ct)
	{
		if (!Register()) return;

		while (!ct.IsCancellationRequested)
		{
			Console.Clear();
			Console.WriteLine("Snake Game! 欢迎游玩贪吃蛇小游戏");
			Console.WriteLine("1. 开始游戏");
			Console.WriteLine("2. 设置菜单");
			Console.WriteLine("3. 退出游戏");
			Console.Write("请选择：");

			switch (Console.ReadKey(intercept: true).Key)
			{
				case ConsoleKey.D1: await ChooseModeAsync(ct); break;
				case ConsoleKey.D2: Setting(); break;
				case ConsoleKey.D3: if (ConfirmExit()) return; break;
			}
		}
	}

	// ───── 注册（替代 Game.Register）─────
	bool Register()
	{
		Console.Clear();
		Console.WriteLine("欢迎来到贪吃蛇游戏！");
		while (true)
		{
			Console.WriteLine("请为你的贪吃蛇设置一个名字：");
			string? name = Console.ReadLine()?.Trim();
			if (string.IsNullOrWhiteSpace(name))
			{
				Console.WriteLine("请输入有效的名字！");
				continue;
			}

			localName = name;
			localId = Guid.NewGuid().ToString("N")[..8];
			Console.WriteLine($"已将你的名字设置为：{name}");
			return true;
		}
	}

	// ───── 模式选择（替代 Game.Start）─────
	async Task ChooseModeAsync(CancellationToken ct)
	{
		Console.Clear();
		Console.WriteLine("请选择游戏模式：");
		Console.WriteLine("1. 单人模式");
		Console.WriteLine("2. 本机双人");
		Console.WriteLine("3. 网络多人（主机）");
		Console.WriteLine("4. 网络多人（加入）");
		Console.WriteLine("5. 返回主菜单");
		Console.Write("请选择：");

		var key = Console.ReadKey(intercept: true).Key;
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
		var env = new GameSystem.RunEnv(renderer, resultView, (ulong)tickMs, random);

		switch (key)
		{
			case ConsoleKey.D1: await RunSingleAsync(env, cts); break;
			case ConsoleKey.D2: await RunDoubleAsync(env, cts); break;
			case ConsoleKey.D3: await RunNetHostAsync(env, cts); break;
			case ConsoleKey.D4: await RunNetClientAsync(cts); break;
		}
	}

	WorldState BuildState(params string[] playerIds)
		=> GameSystem.InitialWorldState(playerIds, MaxTick, width, height, foodNum);

	// ───── 单人 ─────
	async Task RunSingleAsync(GameSystem.RunEnv env, CancellationTokenSource cts)
	{
		var state = BuildState(localId);
		var input = new LocalInput(localId, KeyLayout.Wasd, cts);
		await GameSystem.RunSingleAsync(state, input, env, cts.Token);
	}

	// ───── 本机双人：两套键盘 ─────
	async Task RunDoubleAsync(GameSystem.RunEnv env, CancellationTokenSource cts)
	{
		string guestId = "guest-" + Guid.NewGuid().ToString("N")[..6];
		var state = BuildState(localId, guestId);

		var p1 = new LocalInput(localId, KeyLayout.Wasd, cts);
		var p2 = new LocalInput(guestId, KeyLayout.Arrows, cts);
		await GameSystem.RunDoubleAsync(state, p1, p2, env, cts.Token);
	}

	// ───── 网络主机：1 本地 + 1 远端（可扩成多个 RemoteInput）─────
	async Task RunNetHostAsync(GameSystem.RunEnv env, CancellationTokenSource cts)
	{
		var mqtt = await ConnectMqttAsync();
		if (mqtt is null) return;
		await using var _ = mqtt;

		string guestId = "remote-" + Guid.NewGuid().ToString("N")[..6];
		var state = BuildState(localId, guestId);

		var local = new LocalInput(localId, KeyLayout.Wasd, cts);
		var remote = new RemoteInput(mqtt, guestId);
		await GameSystem.RunNetAsync(state, local, [remote], env, cts.Token);
	}

	// ───── 网络客户端：发 input + 收 state 观战 ─────
	async Task RunNetClientAsync(CancellationTokenSource cts)
	{
		var mqtt = await ConnectMqttAsync();
		if (mqtt is null) return;
		await using var _ = mqtt;

		var input = new LocalInput(localId, KeyLayout.Wasd, cts);
		await ClientRunner.ClientRound(new RunnerContext(mqtt, netOptions, input, renderer, resultView, tickMs, cts.Token));
	}

	async Task<MqttTransport?> ConnectMqttAsync()
	{
		var mqtt = new MqttTransport($"{netOptions.Room}-{Guid.NewGuid().ToString("N")[..18]}")
		{
			Log = Console.WriteLine
		};

		if (!await mqtt.ConnectAsync(netOptions.BrokerHost, netOptions.BrokerPort))
		{
			Console.WriteLine("连接 broker 失败，按任意键返回");
			_ = Console.ReadKey(intercept: true);
			return null;
		}

		await mqtt.SubscribeAsync(default, $"snake/{netOptions.Room}/#");
		return mqtt;
	}

	// ───── 设置（替代 Game.SetMenu + ChangeTime）─────
	void Setting()
	{
		while (true)
		{
			Console.Clear();
			Console.WriteLine("—————设置菜单—————");
			Console.WriteLine($"1、设置地图大小（当前 {width}×{height}）");
			Console.WriteLine($"2、设置初始速度（当前 {tickMs} 毫秒/格）");
			Console.WriteLine($"3、设置游戏时间（当前 {durationSeconds} 秒）");
			Console.WriteLine($"4、修改名字（当前 {localName}）");
			Console.WriteLine("5、退出设置菜单");
			Console.Write("请选择：");

			switch (Console.ReadKey(intercept: true).Key)
			{
				case ConsoleKey.D1: ChangeMapSize(); break;
				case ConsoleKey.D2: ChangeSpeed(); break;
				case ConsoleKey.D3: ChangeDuration(); break;
				case ConsoleKey.D4: Rename(); break;
				default: return;
			}
		}
	}

	void ChangeMapSize()
	{
		Console.Clear();
		Console.WriteLine("1、小地图 20×25  2、中地图 25×30  3、大地图 35×40");
		Console.Write("请选择：");
		(width, height) = Console.ReadKey(intercept: true).Key switch
		{
			ConsoleKey.D1 => (20, 25),
			ConsoleKey.D2 => (25, 30),
			ConsoleKey.D3 => (35, 40),
			_ => (width, height)
		};
		Done();
	}

	void ChangeSpeed()
	{
		Console.Clear();
		Console.WriteLine("1、慢 300ms  2、正常 200ms  3、快 150ms  4、极快 100ms");
		Console.Write("请选择：");
		tickMs = Console.ReadKey(intercept: true).Key switch
		{
			ConsoleKey.D1 => 300,
			ConsoleKey.D2 => 200,
			ConsoleKey.D3 => 150,
			ConsoleKey.D4 => 100,
			_ => tickMs
		};
		Done();
	}

	void ChangeDuration()
	{
		Console.Clear();
		Console.WriteLine("1、一分钟  2、三分钟  3、五分钟");
		Console.Write("请选择：");
		durationSeconds = Console.ReadKey(intercept: true).Key switch
		{
			ConsoleKey.D1 => 60,
			ConsoleKey.D2 => 180,
			ConsoleKey.D3 => 300,
			_ => durationSeconds
		};
		Done();
	}

	void Rename()
	{
		Console.Clear();
		Console.Write("请输入新名字：");
		string? name = Console.ReadLine();
		if (!string.IsNullOrWhiteSpace(name))
			localName = name.Trim();
		Done();
	}

	static void Done()
	{
		Console.WriteLine("已保存，按任意键返回设置菜单…");
		_ = Console.ReadKey(intercept: true);
	}

	// ───── 退出（替代 Game.ExitGame）─────
	static bool ConfirmExit()
	{
		Console.Clear();
		while (true)
		{
			Console.WriteLine("确定要退出游戏吗？ (是/否)");
			string? input = Console.ReadLine();

			if (input is "是") { Console.WriteLine("感谢游玩！"); return true; }

			if (input is "否") return false;

			Console.Clear();
			Console.WriteLine("无效输入，请输入是或否");
		}
	}
}
