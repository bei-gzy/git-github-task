namespace App.Services;

class State
{
	int Score { get; set; }//默认private

	public void SetScore(int score) => Score = score;

	public int GetScore() => Score;

	public record Point(int X, int Y);

	public enum Direction
	{
		Up,
		Down,
		Left,
		Right
	}

	public enum GamingState
	{
		GamingStateRunning,
		GamingGameover,
		Exite
	}
}
