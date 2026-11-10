using App.World;

namespace App.Game.Screen;

class RoomPlayerLists
{
	public const int PlayerMaxCount = 2;

	readonly List<Player> players = [];

	public IReadOnlyList<Player> Players => players;

	public int Count => players.Count;

	public bool AddPlayer(Player player)
	{
		if (players.Count > PlayerMaxCount) return false;

		if (players.Contains(player)) return false;

		players.Add(player);
		return true;
	}

	public bool RemovePlayer(Player player) => players.Remove(player);

	public void Clear() => players.Clear();
}
