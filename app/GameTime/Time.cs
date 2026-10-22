//using App.Interface.State;

namespace App.GameTime;

public class GameTimes
{
	internal double timeLong = 60.0;

	internal DateTime StartTime;

	internal TimeSpan timeLose;

	internal DateTime PasueTime;

	public bool IsPauseTime { get; private set; }

	public async Task<bool> GameStartLoseTime()
	{
		Start();

		await TimePause();

		return IsPauseTime;
	}

	public void Start()
	{
		StartTime = DateTime.Now;
		timeLose = TimeSpan.Zero;
		IsPauseTime = false;
	}

	Task TimePause()
	{
		PasueTime = DateTime.UtcNow;
		timeLose = PasueTime - StartTime;
		if (timeLose >= TimeSpan.FromSeconds(timeLong))
		{
			IsPauseTime = true;
		}

		return Task.CompletedTask;
	}

	public void SetTime(double inputTime) => timeLong = inputTime;
}
