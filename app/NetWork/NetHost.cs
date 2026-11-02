using App.Protocol;
using App.World;

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

	/// <summary>网络是否真的连上（Host 用它判断要不要进游戏）</summary>
	public static bool IsConnected => mqttTransport?.IsConnected ?? false;

	public static void Attach(GameWorld world)
	{
		//依赖没准备好，或者不是主机，就不挂
		if (mqttTransport is null || no is null || no.NetMode is not NetMode.Host) return;

		//已经挂过，幂等返回
		if (sp is not null) return;

		sp = new SnapshotPump(world, no.Room, mqttTransport, 100);
		_ = sp.RunAsync();
		Console.WriteLine("[host]开始广播快照");
	}

	public static async Task RunAsync(NetOptions netOptions)
	{
		no = netOptions;
		//房间名 + “-” + 32位无连字符GUID  ：N不带连字符   表达时 ： [..18]范围运算符，也叫格式
		mqttTransport = new MqttTransport($"{no.Room}-{Guid.NewGuid().ToString("N")[..18]}");
		mqttTransport.Log = Console.WriteLine;//先接日志，连接/订阅的过程才看得到
		if (!await mqttTransport.ConnectAsync(no.BrokerHost, no.BrokerPort))
			return;//连不上就别往下走，由 program.cs 决定是否继续

		await mqttTransport.SubscribeAsync(default, $"snake/{no.Room}/#");

		await (no.NetMode switch
		{
			NetMode.Ping => RunPingAsync(no),
			NetMode.Host => RunHostAsync(),
			NetMode.Client => RunClientAsync(),
			_ => Task.CompletedTask,
		});
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

	static Task RunHostAsync()//主机只负责发快照；收包交给阶段 4 的输入层
	{
		Console.WriteLine("[host] 已连接，进入游戏后开始广播快照");
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
			Drain();
			if (onlyreadone) continue;
			onlyreadone = true;
			_ = await mqttTransport!.PublishAsync($"snake/{no!.Room}/hello", m.GetBytes($"hi,{no.Name}已加入"));//序列化
		}
	}

	static void Drain()//消费收包队列--> 翻译input传回GameWorld
	{
		while (mqttTransport!.TryReceive(out string topic, out byte[] payload))
		{
			//过滤处理,StringComparison    Ordinal这是
			if (!topic.EndsWith("/state", StringComparison.Ordinal)) continue;
			RemoteRender.Draw(GameState.Parser.ParseFrom(payload));
		}

		while (Console.KeyAvailable)
			if (Console.ReadKey(intercept: true).Key is ConsoleKey.Escape) return;
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