//只发意图
using App.Protocol;
using App.Services;

namespace App.NetWork;//局域网的串通

public sealed class RemoteInputSource(State.Direction old) : IInputSource
{
	State.Direction dir = old;
	bool isDash;
	ulong lastTick;

	public void ChangeDirection(PlayerInput m)
	{
		if (m.ClientTick <= lastTick) return;
		lastTick = m.ClientTick;

		var newdir = m.Direction switch
		{
			Direction.Up => dir is not State.Direction.Down ? State.Direction.Up : dir,
			Direction.Down => dir is not State.Direction.Up ? State.Direction.Down : dir,
			Direction.Left => dir is not State.Direction.Right ? State.Direction.Left : dir,
			Direction.Right => dir is not State.Direction.Left ? State.Direction.Right : dir,
			_ => dir
		};

		dir = newdir;

		if (m.UseDash) isDash = true;
	}

	public PlayerIntent Poll()
	{
		bool dash = isDash;
		isDash = false;
		return new PlayerIntent(dir, dash);
	}
}
