//只读只绘制 -- >客户端渲染后续可客户端一套，主一套
using App.World;
using App.Protocol;

namespace App.NetWork;

public static class RemoteRender//proto快照花成终端画面-->客户端渲染
{
	public static bool Draw(GameState state)
	{
		if (state.Width * state.Height is < 100 or > 5000
			|| state.Players.Count > 8
			|| state.Foods.Count > 50)
		{ return false; }

		var ws = ProtoCodec.FromProtoState(state);// 网络消息 → 本地快照

		Console.Clear();
		Console.WriteLine($"————— 观战  房间={state.RoomId}  tick={state.ServerTick} —————");
		MapPrinter.PrintMap(ws);
		MapPrinter.PrintHud(ws);
		if (state.IsGameOver) Console.WriteLine("[对局结束]");
		return true;
	}
}

