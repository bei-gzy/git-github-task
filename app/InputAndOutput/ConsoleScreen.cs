using App.GameCore;

namespace App.InputAndOutput;

public sealed class ConsoleRenderer() : IRenderer//文本输出，书写
{
	//对外的接口->主题画面
	public void Draw(WorldState snapShot, string title, string? hint)
	{
		Console.Clear();
		Console.Write(FrameComposer.Draw(snapShot, title, hint));
	}
}

public sealed class ConsoleResultView() : IRoundResultView
{
	public void ShowResult(RoundResults results)
	{
		Console.Clear();
		Console.ResetColor();
		var ps = results.PlayersResult;
		if (ps.Count is 1)
		{
			Console.WriteLine(ps[0].IsAlive ? "时间到！" : "Gaming Over!");
			Console.WriteLine("得分: " + ps[0].Score);
			Console.WriteLine("长度: " + ps[0].Length);
			Console.WriteLine($"存活: {ps[0].SurviavalTime:F1} 秒");
		}

		else
		{
			var winner = results.Winner;
			Console.WriteLine("————— 对局结束 —————");
			Console.WriteLine(results.IsOver ? "时间到" : winner is null ? "平局" : $"{winner?.Id} 获胜");
			Console.WriteLine($"用时 {results.UseTime:F1} 秒");
			for (int i = 0; i < ps.Count; i++)
			{
				Console.WriteLine($"{ps[i].Id}: {ps[i].Score} 分 长度 {ps[i].Length} {(ps[i].IsAlive ? "存活" : "死亡")}");
			}
		}

		Console.WriteLine();
		Console.WriteLine("按下任意健继续");
		while (Console.KeyAvailable)
			_ = Console.ReadKey(intercept: true);
		_ = Console.ReadKey(intercept: true);
	}
}

