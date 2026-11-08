using App.World;

namespace App.GameScore;

public class PlayerScore
{
	public int MyScore { get; private set; }

	public void AddScore(FoodItem food) => MyScore += food.Type;

	public void ClearScore() => MyScore = 0;
}
