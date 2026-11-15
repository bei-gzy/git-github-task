using App.GameCore;
using App.Infrastructure.Mqtt;
using App.Protocol;

namespace App.InputAndOutput;

public sealed class RemoteInput(MqttTransport mqtt, string id) : IInputSource
{
	public GameCore.PlayerInput? Capture()
	{
		while (mqtt.TryReceive(out string topic, out byte[] payload))
		{
			if (!topic.EndsWith("/input", StringComparison.Ordinal)) continue;
			if (payload.Length is 0 or >= 65536) continue;
			try
			{
				var msg = Protocol.PlayerInput.Parser.ParseFrom(payload);

				return msg.UseDash
					? new GameCore.PlayerInput(new PlayerIntent.Dash(), id)
					: msg.Direction switch
					{
						Direction.Up => new GameCore.PlayerInput(new PlayerIntent.Turn(GameCore.Models.Direction.Up), id),
						Direction.Down => new GameCore.PlayerInput(new PlayerIntent.Turn(GameCore.Models.Direction.Down), id),
						Direction.Left => new GameCore.PlayerInput(new PlayerIntent.Turn(GameCore.Models.Direction.Left), id),
						Direction.Right => new GameCore.PlayerInput(new PlayerIntent.Turn(GameCore.Models.Direction.Right), id),
						_ => null
					};
			}
			catch
			{
				mqtt.Log?.Invoke($"丢弃乱包{payload}");
				payload.AsSpan().Clear();
			}
		}

		return null;
	}
}
