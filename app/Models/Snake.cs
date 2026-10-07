namespace app.Models;
using app.Services;

class SnakeHead //考量私有化哪些
{
    public int HId { get; set; }
    public string? Name { get; set; }

    public List<State.Point> Snake { get;} =
    [
        new (10,15),
        new (10,16),
        new (10,17)
    ];

    //指向头--尾加的那个Point（属性）
    public State.Point Head => Snake[^1];

    public void Move(State.Direction nextdir)
    {
        var head = Head;
        State.Point nextLocation = nextdir switch
        {
            State.Direction.Up => new (head.X, head.Y - 1),
            State.Direction.Down => new (head.X, head.Y + 1),
            State.Direction.Left => new (head.X - 1, head.Y),
            State.Direction.Right => new (head.X + 1, head.Y),            
            _ => head
        };
        Snake.Add(nextLocation);
    }
}
