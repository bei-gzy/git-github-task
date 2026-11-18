using Google.Protobuf;

using App.GameCore;
using App.InputAndOutput;
using App.Protocol;

using App.Infrastructure;
using App.Infrastructure.Mqtt;
using App.Infrastructure.Proto;

namespace App.Application.GameRunner;

public sealed record RunnerContext
(
	MqttTransport Mqtt,
	NetOptions NetOptions,
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
		using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(rct.TickMs));

		var lastDir = GameCore.Models.Direction.Down;
		ulong clientTick = 0;
		GameState? lastState = null;

		while (await timer.WaitForNextTickAsync(rct.Ct))
		{
			// 清除上一帧遗留的本地输入，避免积压按键在本帧被重复处理；随后只采集当前帧输入。
			LocalInput.Clear();
			if (rct.IInput.Capture() is { } input)
			{
				foreach (var i in input)
				{
					var (dir, dash) = i.Intent switch
					{
						PlayerIntent.Turn(var d) => (d, false),
						PlayerIntent.Dash => (lastDir, true),
						_ => (lastDir, false)
					};

					lastDir = dir;

					if (!await rct.Mqtt.PublishAsync($"snake/{rct.NetOptions.Room}/input", ProtoCodec.ToProtoInput(rct.NetOptions, dash, dir, ++clientTick).ToByteArray(), rct.Ct))
						rct.Mqtt.Log?.Invoke("[net] 发布 input 失败");
				}
			}

			while (rct.Mqtt.TryReceive(out string topic, out byte[] payload))
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
				rct.Renderer.Draw(ProtoCodec.FromProtoState(lastState), $"————— 观战 房间={rct.NetOptions.Room} —————", (ulong)rct.TickMs);
		}
	}
}
