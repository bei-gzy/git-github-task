using App.Services;

namespace App.Models;

public class SnakeHead //考量私有化哪些
{
	public List<State.Point> Snake { get; private set; } = [];

	public State.Point Tail => Snake[0];

	//指向头--尾加的那个Point（属性）
	public State.Point Head => Snake[^1];

	public State.Direction Nextdir { get; set; }

	public SnakeHead(int x, int y, State.Direction dir, int length)
	{
		Snake.Clear();
		Nextdir = dir;

		//当前方向的单位向量
		(int dx, int dy) = dir switch
		{
			State.Direction.Left => (-1, 0),
			State.Direction.Right => (1, 0),
			State.Direction.Up => (0, -1),
			State.Direction.Down => (0, 1),
			_ => (0, 0)
		};

		//从尾巴往头铺：Head = Snake[^1] 就是这个点，身体都摆在它身后，
		//这样第一次移动只会往前走，不会一出生就踩到自己
		for (int i = length - 1; i >= 0; i--)
		{
			Snake.Add(new State.Point(x - i * dx, y - i * dy));
		}
	}
}
