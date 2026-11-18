using Google.Protobuf;

using App.Infrastructure.Mqtt;
using App.Infrastructure.Proto;
using App.GameCore;

namespace App.InputAndOutput;

//输出源：把一帧 WorldState 序列化后广播到 MQTT 主题 snake/{room}/state
//由 Round 循环每帧调用 Publish，只负责"发一帧"
public sealed class MqttBroadcast(string roomId, MqttTransport transport)
{
	public async Task Publish(WorldState state, CancellationToken ct = default)
	{
		var msg = ProtoCodec.ToProtoState(state, roomId);
		if (!await transport.PublishAsync($"snake/{roomId}/state", msg.ToByteArray(), ct))
			transport.Log?.Invoke("[mqtt] 发布状态失败");
	}
}
