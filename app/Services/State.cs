namespace app.Services;

class State
{
    public int Score { get; private set;} = 3;

    public void setScore()
    {
        
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