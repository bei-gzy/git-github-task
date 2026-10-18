namespace App.GameScore;

public class PlayerScore
{
	public int MyScore { get; private set; }

	public void AddScore(int score) => MyScore += score;

	public void ClearScore() => MyScore = 0;
}

public class Score
{
	event EventHandler<PlayerScore>? ScoreChanged;
	void AddScore(PlayerScore playerscore, int score)
	{
		playerscore.AddScore(score);
		ScoreChanged?.Invoke(this, playerscore);
	}

	public void CallAddScore(PlayerScore playerscore, int getScore) => AddScore(playerscore, getScore);
}
