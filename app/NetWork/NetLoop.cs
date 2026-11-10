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
			if (topic.EndsWith("/hello", StringComparison.Ordinal))
			{
				Drain(ns, renderer);
				continue;
			}

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

	public static async Task RunAsync(NetSession ns, IRenderer renderer, Func<PlayerIntent?>? poll)
	{
		if (ns is null) return;
		var no = ns.NetOptions;
		switch (no!.NetMode)
		{
			case NetMode.Ping: await RunPingAsync(ns, renderer); return;
			case NetMode.Client: await RunClientAsync(ns, renderer, poll); return;
			case NetMode.Host: break;
			default: break;
		}
	}

	public static async Task RunClientAsync(NetSession ns, IRenderer renderer, Func<PlayerIntent?>? poll = null)
	{
		Console.WriteLine($"[Client]已加入房间 {ns.NetOptions!.Room},WASD 操控玩家2,Q 退出");
		using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

		bool onlyreadone = false;
		var m = System.Text.Encoding.UTF8;
		while (await timer.WaitForNextTickAsync())
		{
			if (poll?.Invoke() is { } intent)
			{
				ns.NextClentTick();
				var input = ProtoCodec.ToProtoInput(ns.NetOptions, intent.IsDash, ProtoCodec.ToProtoDirection(intent.Direction), ns.ClientTick);
				_ = await ns.Transport!.PublishAsync($"snake/{ns.NetOptions.Room}/input", input.ToByteArray());
			}

			Drain(ns, renderer);

			//只在连接成功输出
			if (onlyreadone) continue;
			onlyreadone = true;
			_ = await ns.Transport!.PublishAsync($"snake/{ns.NetOptions.Room}/hello", m.GetBytes($"hi,{ns.NetOptions.Name}已加入"));
		}
	}

	public static async Task RunPingAsync(NetSession ns, IRenderer renderer)
	{
		var m = System.Text.Encoding.UTF8;
		using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
		while (await timer.WaitForNextTickAsync())
		{
			string text = $"hello from {ns.NetOptions!.Name} @ {DateTime.UtcNow:HH:mm:ss}";
			if (await ns.Transport!.PublishAsync($"snake/{ns.NetOptions!.Room}/hello", m.GetBytes(text)))
				Console.WriteLine($"[ping]发送: {text}");
			Drain(ns, renderer);
		}
	}
}
