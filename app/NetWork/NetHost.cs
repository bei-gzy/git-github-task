using App.Protocol;
using App.World;
using App.KeyBoard;

using Google.Protobuf;

namespace App.NetWork;

//控制网络的生命周期
public static class NetHost//启动器 + 分派器（组装器）
{
	/*全局状态容器
	一次性初始化：在 RunAsync 里创建 MqttTransport、连接 Broker、订阅主题，然后才进入各模式的主循环。连接和订阅只做一次，三个模式共享。
	三种模式分流
	mqtttransport收包后台异步调用，主循环用自定义时间控制器消费、调用*/

	//用静态字段 transport 和 options 保存当前会话的传输对象和配置
	static MqttTransport? mqttTransport;
	static NetOptions? no;
	static SnapshotPump? sp;
	static Task? snapShotTask;
	static Button? button;
	static ulong ClentTick { get; }

	public static void StartSnapShot(GameWorld world)
	{
		if (mqttTransport is null || no is null || no.NetMode is not NetMode.Host || sp is not null) return;
		sp = new SnapshotPump(world, no.Room, mqttTransport, 100);
		snapShotTask = sp.RunAsync();
		//"[host]开始广播快照"
	}

	public static async Task DetachAsync()
	{
		try
		{
			if (snapShotTask is not null)
				await snapShotTask.ConfigureAwait(false);
		}
		finally
		{
			try
			{
				sp!.Dispose();
				no!.ReSet();
			}
			finally
			{
				if (mqttTransport is not null)
					await mqttTransport.DisposeAsync().ConfigureAwait(false);
				// 记录日志，dispose 阶段一般不往外抛
				mqttTransport!.Log?.Invoke($"[host]断开连接");
			}
		}
	}

	public static async Task RunAsync(NetOptions netOptions)
	{
		no = netOptions;
		//房间名 + “-” + 32位无连字符GUID  ：N不带连字符   表达时 ： [..18]范围运算符，也叫格式
		mqttTransport = new MqttTransport($"{no.Room}-{Guid.NewGuid().ToString("N")[..18]}");
		if (await mqttTransport.ConnectAsync(no.BrokerHost, no.BrokerPort))
		{
			await mqttTransport.SubscribeAsync(default, $"snake/{no.Room}/#");
			mqttTransport.Log = Console.WriteLine;

			await (no.NetMode switch
			{
				NetMode.Ping => RunPingAsync(no),
				NetMode.Host => RunHostAsync(),
				NetMode.Client => RunClientAsync(),
				_ => Task.CompletedTask,
			});
		}
	}

	static async Task RunPingAsync(NetOptions no)
	{
		var m = System.Text.Encoding.UTF8;
		using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
		while (await timer.WaitForNextTickAsync())
		{
			string text = $"hello from {no!.Name} @ {DateTime.UtcNow:HH:mm:ss}";
			if (await mqttTransport!.PublishAsync($"snake/{no!.Room}/hello", m.GetBytes(text)))//序列化
				Console.WriteLine($"[ping]发送: {text}");
			Drain();
		}
	}

	static Task RunHostAsync()//只收不发，本机自处理
	{
		if (mqttTransport is not null || no is not null || sp is not null)
			Console.WriteLine($"已经就绪，进游戏后广播");
		return Task.CompletedTask;
	}

	static async Task RunClientAsync()//先发一次，然后只收
	{
		Console.WriteLine($"[Client]已加入房间，等待主机消息...(Ctrl + c)退出");
		using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

		bool onlyreadone = false;
		var m = System.Text.Encoding.UTF8;
		while (await timer.WaitForNextTickAsync())
		{
			await ClientInput();
			Drain();
			//只在连接成功输出
			if (onlyreadone) continue;
			onlyreadone = true;
			_ = await mqttTransport!.PublishAsync($"snake/{no!.Room}/hello", m.GetBytes($"hi,{no.Name}已加入"));
		}

		while (Console.KeyAvailable)
			if (Console.ReadKey(intercept: true).Key is ConsoleKey.Escape) return;
	}

	static async Task ClientInput()
	{
		while (Console.KeyAvailable)
		{
			var key = Console.ReadKey(intercept: true).Key;
			var (dir, isDash) = key switch
			{
				ConsoleKey.W or ConsoleKey.UpArrow => (Direction.Up, false),
				ConsoleKey.S or ConsoleKey.DownArrow => (Direction.Down, false),
				ConsoleKey.A or ConsoleKey.LeftArrow => (Direction.Left, false),
				ConsoleKey.D or ConsoleKey.RightArrow => (Direction.Right, false),
				ConsoleKey.E or ConsoleKey.NumPad0 => (Direction.Unspecified, true),
				_ => (Direction.Unspecified, false)
			};

			if (!isDash && dir is Direction.Unspecified) continue;
			if (no is null) continue;
			var input = ProtoCodec.ToProtoInput(no, isDash, dir, ClentTick);

			if (!await mqttTransport!.PublishAsync($"snake/{no!.Room}/input", input.ToByteArray()))
				mqttTransport.Log?.Invoke($"[client]发送: {key} {input}失败");
		}

		return;
	}

	public static void PumpInput(Button input)
	{
		button = input;
		if (mqttTransport is null || no is null || button is null) return;
		while (mqttTransport.TryReceive(out string topic, out byte[] payload))
		{
			if (!topic.EndsWith("/input", StringComparison.Ordinal)) continue;
			try
			{
				if (button is not null && payload.Length is > 0 and < 65536)
					RemoteInput.ReadRemoteInput(button, no.Room, PlayerInput.Parser.ParseFrom(payload));
			}
			catch
			{
				mqttTransport.Log?.Invoke($"丢弃乱包{payload}");
				payload.AsSpan().Clear();
			}
		}
	}

	static void Drain()//消费收包队列--> 翻译input传回GameWorld
	{
		while (mqttTransport!.TryReceive(out string topic, out byte[] payload))
		{
			//过滤处理,StringComparison      Ordinal 按字符编码值进行区分大小写的比较，不受语言/区域设置影响；适合比较协议主题。
			// OrdinalIgnoreCase 可忽略大小写；CurrentCulture 会按当前区域规则比较，通常不适合协议标识。
			if (!topic.EndsWith("/state", StringComparison.Ordinal)) continue;
			byte[] lastPayload = payload;
			try
			{
				if (lastPayload.Length is > 0 and < 65536 && RemoteRender.Draw(GameState.Parser.ParseFrom(lastPayload)))
					mqttTransport.Log?.Invoke($"[render]成功绘制: {topic}");
			}
			catch
			{
				mqttTransport.Log?.Invoke($"丢弃乱包{payload}");
				payload.AsSpan().Clear();
			}
		}
	}
}

/*
NetOptions（包） ──┐
MqttTransport（传输） ──┼──► NetHost.RunAsync ──► 按 Mode 分派到 Ping/Host/Client
Console（输出）     ──┘


Main
 └─ NetOptions.Parse(args, "snake-dev")  → Mode=Host, Room=my-room, Name=alice
      └─ NetHost.RunAsync(o)
           ├─ options = o
           ├─ transport = new MqttTransport("my-room-<GUID>"[..18])
           ├─ await transport.ConnectAsync("broker.emqx.io", 1883)
           ├─ await transport.SubscribeAsync("snake/my-room/#")
           ├─ 打印配置和引导
           └─ switch → RunHostAsync(o)
                └─ while (每 100ms)
                     └─ Drain() → 从队列取消息打印


RunAsync → ... → RunClientAsync(o)
     └─ while (每 100ms)
          ├─ Drain()
          └─ 第一轮：PublishAsync("snake/my-room/hello", "hi, bob 已加入")
*/
