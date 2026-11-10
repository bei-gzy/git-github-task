namespace App.Services;

public readonly record struct PlayerIntent(State.Direction Direction, bool IsDash);
public interface IInputSource//这个端口没毛
{
	PlayerIntent Poll();
}
