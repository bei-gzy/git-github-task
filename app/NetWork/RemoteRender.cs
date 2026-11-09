//只读只绘制 -- >客户端渲染后续可客户端一套，主一套
using App.Protocol;
using App.World;

namespace App.NetWork;

public static class RemoteRender//proto快照花成终端画面-->客户端渲染
{
	public static bool Build(GameState state, out WorldSnapShot snapShot)
	{
		snapShot = default!;
		if (state.Width * state.Height is < 100 or > 5000
			|| state.Players.Count > 8
			|| state.Foods.Count > 50)
		{ return false; }

		snapShot = ProtoCodec.FromProtoState(state);
		return true;
	}
}

