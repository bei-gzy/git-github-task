using Game.Register;
namespace Game.Screen;

public class PlayerLists
{
	public List<Player> Players { get; private set; } = [];

	const int PlayerMaxCount = 2;

	public void AddPlayer(Player player)
	{
		Players.Add(player);
		if (Players.Count is PlayerMaxCount)
		{
			Console.WriteLine("人数已满！");
		}
	}
}
