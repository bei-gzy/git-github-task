namespace Game.Register;

public class Player(string? sname)
{
	public string? Name { get; private set; } = sname;

	public void Rename(string? name) => Name = name;

	//随即生成ID
}
