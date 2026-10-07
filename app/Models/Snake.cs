namespace app.Models;
using app.Services;

class SnakeHead //考量私有化哪些
{
    public int HId { get; set; }
    public string? Name { get; set; }

    public List<State.Point> Snake { get;} =
    [
        new (0,0),
        new (0,1),
        new (0,2)
    ];

    public int Score { get; } = 0;

    //指向头--尾加的那个Point（属性）
    public State.Point Head => Snake[^1];

    public void Move(State.Direction nextdir,State.Point newHead,State.Point nextLocation)
    {
        nextLocation = nextdir switch
        {
            State.Direction.Up => new (Head.X, Head.Y - 1),
            State.Direction.Down => new (Head.X, Head.Y + 1),
            State.Direction.Left => new (Head.X - 1, Head.Y),
            State.Direction.Right => new (Head.X + 1, Head.Y),            
            _ => Head
        };
        newHead = nextLocation;
    }
}
