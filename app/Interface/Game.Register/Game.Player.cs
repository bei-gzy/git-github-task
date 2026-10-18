namespace Game.Register;

public class Player(string? name)
{
	public string? Name { get; private set; } = name;

	public void Rename(string? name) => Name = name;

	//随即生成ID

	public void SetSnakeName(string? sname)
	{
		Console.WriteLine("请设置你的名字: ");
		while (true)
		{
			sname = Console.ReadLine();
			if (string.IsNullOrEmpty(sname))
			{
				Console.WriteLine("你设置的名字不能为空，请重新设置: ");
				continue;
			}
			else
			{
				break;
			}
		}

		Name = sname;
	}
}
