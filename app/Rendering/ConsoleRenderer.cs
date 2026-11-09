using App.Services;
using App.World;

namespace App.Rendering;

public sealed class ConsoleRenderer(TextWriter tw) : IRenderer//文本输出，书写
{
	//对外的接口->主题画面
	public void Draw(WorldSnapShot snapShot, string title, string? hint)
	{
		Console.Clear();
		tw.Write(FrameComposer.Draw(snapShot, title, hint));
	}
}

public sealed class ConsoleResultView(TextWriter tw, TextReader tr) : IRoundResultView
{
	public void ShowResult(RoundResult result)
	{
		Console.Clear();
		Console.ResetColor();
		foreach (string line in result.Lines)
			tw.WriteLine(line);
		tw.WriteLine();
		tw.WriteLine("按下任意健继续");
	}

	public void WaitForNext() => tr.Read();//读取用户按键
}
