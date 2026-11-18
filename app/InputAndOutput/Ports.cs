using App.GameCore;

namespace App.InputAndOutput;

//端口
public interface IRenderer
{
	void Draw(WorldState snapShot, string title, ulong tickMs, string? hint = null);
}

//端口--结算
public interface IRoundResultView
{
	void ShowResult(RoundResults results);
}

public interface IInputSource
{
	IReadOnlyList<PlayerInput>? Capture();
}

