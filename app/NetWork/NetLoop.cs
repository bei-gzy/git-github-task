using App.Protocol;
using App.Services;

using Google.Protobuf;

namespace App.NetWork;

public sealed class NetLoop
{
	static void Drain(NetSession ns, IRenderer renderer)//消费收包队列 -> 只画最新一帧
	{
		GameState? latest = null;

		while (ns.Transport!.TryReceive(out string topic, out byte[] payload))
		{
			if (!topic.EndsWith("/state", StringComparison.Ordinal)) continue;
			if (payload.Length is 0 or >= 65536) continue;

			try
			{
				var st = GameState.Parser.ParseFrom(payload);
				if (latest is null || st.ServerTick >= latest.ServerTick)
					latest = st;
			}
			catch
			{
				ns.Transport.Log?.Invoke($"[net] 丢弃坏包 {topic} ({payload.Length}B)");
			}
		}

		if (latest is not null && RemoteRender.Build(latest, out var ws))
			renderer.Draw(ws, $"————— 观战  房间={latest.RoomId}  tick={latest.ServerTick} —————");
	}

	static async Task ClientInput(NetSession ns)
	{
		var no = ns.NetOptions;
		var transport = ns.Transport;
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
			ns.NextClentTick();
			var input = ProtoCodec.ToProtoInput(no, isDash, dir, ns.ClientTick);

			if (!await transport!.PublishAsync($"snake/{no!.Room}/input", input.ToByteArray()))
				transport.Log?.Invoke($"[client]发送: {key} {input}失败");
		}

		return;
	}

	public static async Task RunAsync(NetSession ns, IRenderer renderer)
	{
		if (ns is null) return;
		var no = ns.NetOptions;
		switch (no!.NetMode)
		{
			case NetMode.Ping: await RunPingAsync(ns, renderer); return;
			case NetMode.Client: await RunClientAsync(ns, renderer); return;
			case NetMode.Host: break;
			default: break;
		}
	}

	public static async Task RunClientAsync(NetSession ns, IRenderer renderer)//先发一次，然后只收
	{
		Console.WriteLine($"[Client]已加入房间，等待主机消息...(Ctrl + c)退出");
		using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

		bool onlyreadone = false;
		var m = System.Text.Encoding.UTF8;
		while (await timer.WaitForNextTickAsync())
		{
			await ClientInput(ns);
			Drain(ns, renderer);
			//只在连接成功输出
			if (onlyreadone) continue;
			onlyreadone = true;
			_ = await ns.Transport!.PublishAsync($"snake/{ns.NetOptions!.Room}/hello", m.GetBytes($"hi,{ns.NetOptions.Name}已加入"));
			while (Console.KeyAvailable)
				if (Console.ReadKey(intercept: true).Key is ConsoleKey.Escape) return;
		}
	}

	public static async Task RunPingAsync(NetSession ns, IRenderer renderer)
	{
		var m = System.Text.Encoding.UTF8;
		using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
		while (await timer.WaitForNextTickAsync())
		{
			string text = $"hello from {ns.NetOptions!.Name} @ {DateTime.UtcNow:HH:mm:ss}";
			if (await ns.Transport!.PublishAsync($"snake/{ns.NetOptions!.Room}/hello", m.GetBytes(text)))//序列化
				Console.WriteLine($"[ping]发送: {text}");
			Drain(ns, renderer);
		}
	}
}
