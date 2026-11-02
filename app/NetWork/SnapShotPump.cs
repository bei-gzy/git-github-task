using App.World;

using Google.Protobuf;

namespace App.NetWork;

public sealed class SnapshotPump(GameWorld world, string roomId, MqttTransport transport, int intervalMs = 100) : IDisposable
{
	readonly CancellationTokenSource cts = new();

	public async Task RunAsync()
	{
		using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));

		try
		{
			while (await timer.WaitForNextTickAsync(cts.Token))
			{
				if (world.Players.Count is 0) continue;

				var msg = ProtoCodec.ToProto(world.Snapshot(), roomId);

				if (!await transport.PublishAsync($"snake/{roomId}/state", msg.ToByteArray(), cts.Token))
					transport.Log?.Invoke("[mqtt] 发布快照失败");
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	public void Dispose()//舍弃throw new NotImplementedException
	{
		cts.Cancel();
		cts.Dispose();
	}
}
