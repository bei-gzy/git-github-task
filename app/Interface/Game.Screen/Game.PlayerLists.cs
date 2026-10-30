using App.World;

namespace App.Game.Screen;

//房间座位表：0.8 联机建房/加入时用，现在本机双人也会过一遍
class RoomPlayerLists
{
	public const int PlayerMaxCount = 2;

	readonly List<Player> players = [];

	public IReadOnlyList<Player> Players => players;

	public int Count => players.Count;

	public bool IsFull => players.Count >= PlayerMaxCount;

	public bool AddPlayer(Player player)
	{
		if (IsFull)
		{
			Console.WriteLine("人数已满！");
			return false;
		}

		if (players.Contains(player)) return false;

		players.Add(player);
		return true;
	}

	public bool RemovePlayer(Player player) => players.Remove(player);

	public void Clear() => players.Clear();
}
