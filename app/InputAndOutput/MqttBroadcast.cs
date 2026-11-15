using Google.Protobuf;

using App.Infrastructure.Mqtt;
using App.Infrastructure.Proto;
using App.GameCore;

namespace App.InputAndOutput;

public sealed class MqttBroadcast(string roomId, MqttTransport transport, int intervalMs = 100) : IDisposable
{
	readonly CancellationTokenSource cts = new();

	public async Task RunAsync(WorldState snap, CancellationToken ct = default)//主机发快照给客户端
	{
		using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));

		try
		{
			while (await timer.WaitForNextTickAsync(cts.Token))
			{
				var msg = ProtoCodec.ToProtoState(snap, roomId);
				if (!await transport.PublishAsync($"snake/{roomId}/state", msg.ToByteArray(), cts.Token))
					transport.Log?.Invoke("[mqtt] 发布状态失败");
			}
		}
		catch (OperationCanceledException) { }
	}

	public void Dispose()//舍弃throw new NotImplementedException
	{
		cts.Cancel();
		cts.Dispose();
	}
}

