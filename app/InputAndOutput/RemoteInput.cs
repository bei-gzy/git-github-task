using App.GameCore;
using App.Infrastructure.Mqtt;
using App.Protocol;

namespace App.InputAndOutput;

public sealed class RemoteInput(MqttTransport mqtt) : IInputSource
{
	public IReadOnlyList<GameCore.PlayerInput> Capture()
	{
		var inputs = new List<GameCore.PlayerInput>();
		while (mqtt.TryReceive(out string topic, out byte[] payload))
		{
			if (!topic.EndsWith("/input", StringComparison.Ordinal)) continue;
			if (payload.Length is 0 or >= 65536) continue;
			try
			{
				var msg = Protocol.PlayerInput.Parser.ParseFrom(payload);

				var input = msg.UseDash
					? new GameCore.PlayerInput(new PlayerIntent.Dash(), msg.PlayerId)
					: msg.Direction switch
					{
						Direction.Up => new GameCore.PlayerInput(new PlayerIntent.Turn(GameCore.Models.Direction.Up), msg.PlayerId),
						Direction.Down => new GameCore.PlayerInput(new PlayerIntent.Turn(GameCore.Models.Direction.Down), msg.PlayerId),
						Direction.Left => new GameCore.PlayerInput(new PlayerIntent.Turn(GameCore.Models.Direction.Left), msg.PlayerId),
						Direction.Right => new GameCore.PlayerInput(new PlayerIntent.Turn(GameCore.Models.Direction.Right), msg.PlayerId),
						_ => null
					};
				if (input is not null)
					inputs.Add(input);
			}
			catch
			{
				mqtt.Log?.Invoke($"丢弃乱包{payload}");
				payload.AsSpan().Clear();
			}
		}

		return inputs;
	}
}
