using App.World;

namespace App.Services;

//端口
public interface IRenderer
{
	void Draw(WorldSnapShot snapShot, string title, string? hint = null);
}

//端口--结算
public readonly record struct RoundResult(bool Died, IReadOnlyList<string> Lines);
public interface IRoundResultView
{
	void ShowResult(RoundResult result);
	void WaitForNext();
}
