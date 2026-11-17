using App.GameCore;
using App.Infrastructure;
using App.Infrastructure.Mqtt;
using App.Infrastructure.Proto;
using App.InputAndOutput;
using App.Protocol;

namespace App.Application.GameRunner;

public sealed record RunnerContext
(
	string? Hint,
	string Title,
	MqttTransport Mqtt,
	NetOptions NetOptions,
	WorldState State,
	IInputSource IInput,
	IRenderer Renderer,
	IRoundResultView ResultView,
	int TickMs,
	CancellationToken Ct
);

//捕获键盘 -- 通过Mqtt发input -- 收state -- 渲染
public static class ClientRunner
{
	public static async Task ClientRound(RunnerContext rct)
	{
		Console.WriteLine($"[Client]已加入房间 {rct.NetOptions!.Room},WASD 操控玩家2,Q 退出");
		using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(rct.TickMs));

		var lastDir = GameCore.Models.Direction.Down;
		ulong clientTick = 0;
		bool onlyreadone = false;
		var m = System.Text.Encoding.UTF8;

		while (await timer.WaitForNextTickAsync(rct.Ct))
		{
			if (rct.IInput.Capture() is { } input)
			{
				var (dir, dash) = input.Intent switch
				{
					PlayerIntent.Turn(var d) => (d, false),
					PlayerIntent.Dash => (lastDir, true),
					_ => (lastDir, false)
				};

				lastDir = dir;

				_ = await rct.Mqtt.PublishAsync($"snake/{rct.NetOptions.Room}/input", ProtoCodec.ToProtoInput(rct.NetOptions, dash, dir, ++clientTick).ToByteArray(), rct.Ct);
			}

			GameState? lastState = null;

			while (!rct.Mqtt.TryReceive(out string topic, out byte[] payload))
			{
				if (payload.Length is 0 or >= 65536) continue;

				if (!topic.EndsWith("/state", StringComparison.Ordinal)) continue;
				try
				{
					var st = GameState.Parser.ParseFrom(payload);
					if (lastState is null || st.ServerTick >= lastState.ServerTick)
						lastState = st;
				}
				catch
				{
					rct.Mqtt.Log?.Invoke($"[net] 丢弃坏包 {topic} ({payload.Length}B)");
				}
			}

			if (lastState is not null)
				rct.Renderer.Draw(ProtoCodec.FromProtoState(lastState), rct.Title, (ulong)rct.TickMs, rct.Hint);

			//只在首次连接成功输出
			if (onlyreadone) continue;
			onlyreadone = true;
			_ = await rct.Mqtt!.PublishAsync($"snake/{rct.NetOptions.Room}/hello", m.GetBytes($"hi,{rct.NetOptions.Name}已加入"));
		}
	}
}