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

		// 命令行 --host/--join/--ping 指定了网络模式：直接分流，不进主菜单
		if (netOptions.NetMode != NetMode.None)
		{
			await DispatchNetAsync(ct);
			return;
		}

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

	// ───── 统一分流：根据 NetMode 进入对应网络模式（菜单选 3/4 与命令行共用）─────
	async Task DispatchNetAsync(CancellationToken ct)
	{
		switch (netOptions.NetMode)
		{
			case NetMode.Host: await RunHostDirectly(ct); break;
			case NetMode.Client: await RunClientDirectly(ct); break;
			case NetMode.Ping: await RunPingDirectly(ct); break;
		}
	}

	// ───── 直接开主机（--host）：跳过菜单与注册，用 --name 作本地身份 ─────
	public async Task RunHostDirectly(CancellationToken ct)
	{
		localName = netOptions.Name;
		localId = netOptions.Name;
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
		var env = new GameSystem.RunEnv(renderer, resultView, (ulong)tickMs, random);
		await RunNetHostAsync(env, cts);
	}

	// ───── 直接加入（--join）：只收发（发 input + 收 state 渲染）─────
	public async Task RunClientDirectly(CancellationToken ct)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
		await RunNetClientAsync(cts);
	}

	// ───── 连通测试（--ping）─────
	public async Task RunPingDirectly(CancellationToken ct)
	{
		await using var mqtt = new MqttTransport($"ping-{Guid.NewGuid():N}"[..23]) { Log = Console.WriteLine };
		Console.WriteLine($"连接 {netOptions.BrokerHost}:{netOptions.BrokerPort} ...");
		if (!await mqtt.ConnectAsync(netOptions.BrokerHost, netOptions.BrokerPort, ct: ct))
		{
			Console.WriteLine("[ping] 连接失败");
			return;
		}

		await mqtt.SubscribeAsync(ct, $"snake/{netOptions.Room}/#");
		if (!await mqtt.PublishAsync($"snake/{netOptions.Room}/hello",
			System.Text.Encoding.UTF8.GetBytes($"ping {DateTime.UtcNow:HH:mm:ss}"), ct))
		{
			Console.WriteLine("[ping] 发送 hello 失败");
		}

		var deadline = DateTime.UtcNow.AddSeconds(3);
		while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
		{
			if (mqtt.TryReceive(out string topic, out byte[] payload))
			{
				Console.WriteLine($"[ping] 收到回包: {topic} {System.Text.Encoding.UTF8.GetString(payload)}");
				return;
			}

			await Task.Delay(100, ct);
		}

		Console.WriteLine("[ping] 超时(broker 已通但无回包）");
	}

	// ───── 注册 ─────
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
			netOptions.Name = name;
			Console.WriteLine($"已将你的名字设置为：{name}");
			return true;
		}
	}

	// ───── 模式选择 ─────
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
			case ConsoleKey.D3:
				netOptions.NetMode = NetMode.Host;   // 标注状态
				await DispatchNetAsync(ct);
				netOptions.NetMode = NetMode.None;   // 复位
				break;
			case ConsoleKey.D4:
				netOptions.NetMode = NetMode.Client;
				await DispatchNetAsync(ct);
				netOptions.NetMode = NetMode.None;
				break;
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

	// ───── 网络主机：1 本地 + 远端 ─────
	async Task RunNetHostAsync(GameSystem.RunEnv env, CancellationTokenSource cts)
	{
		var mqtt = await ConnectMqttAsync();
		if (mqtt is null) return;
		await using var _ = mqtt;

		// 等待玩家加入
		Console.Clear();
		Console.WriteLine($"主机已就绪，房间 {netOptions.Room}");
		Console.WriteLine("等待玩家加入...（按 Esc 取消）");

		if (cts.IsCancellationRequested) return;

		Dictionary<string, PlayerState> remotePlayers = [];
		string? guestId = await WaitForPlayerAsync(mqtt, cts);
		if (guestId is null) return;

		var state = BuildState(localId);
		var local = new LocalInput(localId, KeyLayout.Wasd, cts);
		var remote = new RemoteInput(mqtt);
		var broadcast = new MqttBroadcast(netOptions.Room, mqtt);
		await GameSystem.RunNetAsync(state, local, [remote], env with { Broadcast = broadcast }, cts.Token);
	}

	// ───── 网络客户端：发 input + 收 state 观战 ───── 展望未来
	async Task RunNetClientAsync(CancellationTokenSource cts)
	{
		var mqtt = await ConnectMqttAsync();
		if (mqtt is null) return;
		await using var _ = mqtt; //mqtt 方法结束时Dispose，释放连接

		// 先发 hello 宣布加入，再等主机
		Console.Clear();
		Console.WriteLine($"已加入房间 {netOptions.Room}");
		Console.WriteLine("正在等待主机开始游戏...（按 Esc 取消）");
		if (!await mqtt.PublishAsync($"snake/{netOptions.Room}/hello",
			System.Text.Encoding.UTF8.GetBytes($"hi,{netOptions.Name}已加入"), cts.Token))
		{
			Console.WriteLine("[net] 发布 hello 失败");
		}

		await WaitForHostAsync(mqtt, cts);

		if (cts.IsCancellationRequested) return;

		var input = new LocalInput(netOptions.Name, KeyLayout.Wasd, cts);
		await ClientRunner.ClientRound(new RunnerContext(mqtt, netOptions, input, renderer, resultView, tickMs, cts.Token));
	}

	// 主机：等待客户端 hello/input，或 Esc 取消，或 30 秒超时
	static async Task<string?> WaitForPlayerAsync(MqttTransport mqtt, CancellationTokenSource cts)
	{
		var deadline = DateTime.UtcNow.AddSeconds(30);
		while (DateTime.UtcNow < deadline && !cts.IsCancellationRequested)
		{
			if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Escape)
			{
				cts.Cancel();
				return null;
			}

			while (mqtt.TryReceive(out string topic, out byte[] payload))
			{
				if (topic.EndsWith("/input", StringComparison.Ordinal))
				{
					var msg = Protocol.PlayerState.Parser.ParseFrom(payload);
					Console.WriteLine("玩家已加入，开始游戏！");
					return msg.PlayerId;
				}
			}

			await Task.Delay(100, cts.Token);
		}

		if (!cts.IsCancellationRequested)
			Console.WriteLine("等待超时，开始游戏");
		return "remote";
	}

	// 客户端：等待主机 /state，或 Esc 取消，或 30 秒超时
	static async Task WaitForHostAsync(MqttTransport mqtt, CancellationTokenSource cts)
	{
		var deadline = DateTime.UtcNow.AddSeconds(30);
		while (DateTime.UtcNow < deadline && !cts.IsCancellationRequested)
		{
			if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Escape)
			{
				cts.Cancel();
				return;
			}

			while (mqtt.TryReceive(out string topic, out _))
			{
				if (topic.EndsWith("/state", StringComparison.Ordinal))
				{
					Console.WriteLine("主机已开始，进入游戏！");
					return;
				}
			}

			await Task.Delay(100, cts.Token);
		}

		if (!cts.IsCancellationRequested)
			Console.WriteLine("等待超时");
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

	// ───── 设置 ─────
	void Setting()
	{
		while (true)
		{
			Console.Clear();
			Console.WriteLine("—————设置菜单—————");
			Console.WriteLine($"1、设置地图大小(当前 {width}×{height})");
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

	// ───── 退出 ─────
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
