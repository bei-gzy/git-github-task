using App.Services;
namespace App.Models;

public class SnakeHead //考量私有化哪些
{
	public int HId { get; set; }//用于排名

	public string? Name { get; set; }

	public List<State.Point> Snake { get; private set; } =
	[
		new (10,15),
		new (10,16),
		new (10,17)
	];

	public State.Point Tail => Snake[0];

	//指向头--尾加的那个Point（属性）
	public State.Point Head => Snake[^1];

	public void Move(State.Direction nextdir)
	{
		var head = Head;
		var nextLocation = nextdir switch
		{
			State.Direction.Up => new(head.X, head.Y - 1),
			State.Direction.Down => new(head.X, head.Y + 1),
			State.Direction.Left => new(head.X - 1, head.Y),
			State.Direction.Right => new(head.X + 1, head.Y),
			_ => head//不会走到这一步
		};
		Snake.Add(nextLocation);
	}
}
