//只读只绘制 -- >客户端渲染后续可客户端一套，主一套
using App.World;
using App.Protocol;

namespace App.NetWork;

public static class RemoteRender//proto快照花成终端画面-->客户端渲染
{
	public static void Draw(GameState state)
	{
		var ws = ProtoCodec.FromProto(state);// 网络消息 → 本地快照

		Console.Clear();
		Console.WriteLine($"————— 观战  房间={state.RoomId}  tick={state.ServerTick} —————");
		MapPrinter.PrintMap(ws);
		MapPrinter.PrintHud(ws);
		if (state.IsGameOver) Console.WriteLine("[对局结束]");
	}
}

