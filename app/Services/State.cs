namespace app.Services;

class State
{
    private int Score { get; set;}

    public void SetScore(int score)
    {
        this.Score = score;
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