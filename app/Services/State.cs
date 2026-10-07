namespace app.Services;

class State
{
    private int Score { get; set;}

    public void SetScore(int score)
    {
        if(score > 3)
            this.Score = score;
        else if
           (score <= 3)
            DefeatSystem.Defeat(this);
    }

    public int GetScore()
    {
        return this.Score;
    }

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