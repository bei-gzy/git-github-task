using App.Protocol;
using App.World;
using App.KeyBoard;

namespace App.NetWork;

//控制网络的生命周期
//启动器 + 分派器（组装器）
/*全局状态容器
一次性初始化：在 RunAsync 里创建 MqttTransport、连接 Broker、订阅主题，然后才进入各模式的主循环。连接和订阅只做一次，三个模式共享。
三种模式分流
mqtttransport收包后台异步调用，主循环用自定义时间控制器消费、调用*/
public sealed class NetSession(MqttTransport mt, NetOptions no) : IAsyncDisposable
{
	//连接， 订阅， 发布， 收件箱， Dispose
	public MqttTransport? Transport { get; } = mt;
	public NetOptions? NetOptions { get; } = no;
	SnapshotPump? pump;//发布
	Task? snapShotTask;
	Button? button;
	public ulong ClientTick { get; private set; }

	public void NextClentTick() => ClientTick++;

	public static async Task<NetSession> StartAsync(NetOptions no)//通过异步完成初始化并完成连接和订阅
	{
		//房间名 + “-” + 32位无连字符GUID  ：N不带连字符   表达时 ： [..18]范围运算符，也叫格式
		var mt = new MqttTransport($"{no.Room}-{Guid.NewGuid().ToString("N")[..18]}")
		{
			Log = Console.WriteLine
		};
		if (await mt.ConnectAsync(no.BrokerHost, no.BrokerPort))
		{
			await mt.SubscribeAsync(default, $"snake/{no.Room}/#");
		}

		return new NetSession(mt, no);
	}

	public void PublishSnapShot(GameWorld world)
	{
		if (Transport is null || NetOptions is null || NetOptions.NetMode is not NetMode.Host || pump is not null) return;
		pump = new SnapshotPump(world, NetOptions.Room, Transport, 100);
		snapShotTask = pump.RunAsync();
		//"[host]开始广播快照"
	}

	public void PumpInput(Button input)
	{
		button = input;
		if (Transport is null || NetOptions is null || button is null) return;
		while (Transport.TryReceive(out string topic, out byte[] payload))
		{
			if (!topic.EndsWith("/input", StringComparison.Ordinal)) continue;
			try
			{
				if (button is not null && payload.Length is > 0 and < 65536)
					RemoteInput.ReadRemoteInput(button, NetOptions.Room, PlayerInput.Parser.ParseFrom(payload));
			}
			catch
			{
				Transport.Log?.Invoke($"丢弃乱包{payload}");
				payload.AsSpan().Clear();
			}
		}
	}

	public void StopSnapShot()
	{
		pump?.Dispose();
		pump = null;
		snapShotTask = null;
	}

	public async ValueTask DisposeAsync()
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
				pump?.Dispose();
				pump = null;
				NetOptions?.ReSet();
			}
			finally
			{
				if (Transport is not null)
					await Transport.DisposeAsync().ConfigureAwait(false);
				// 记录日志，dispose 阶段一般不往外抛
				Transport!.Log?.Invoke($"[host]断开连接");
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
