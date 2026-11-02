using App.Protocol;
using App.World;

namespace App.NetWork;

public static class RemoteRender
{
	public static void Draw(GameState state) => MapPrinter.Draw(state);
}
