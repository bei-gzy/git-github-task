using App.GameCore.Models;
namespace App.GameCore;

//抽象 record + 嵌套的 sealed record
public abstract record PlayerIntent
{
	public sealed record Turn(Direction Dir) : PlayerIntent;
	public sealed record Dash : PlayerIntent;
}

public record PlayerInput(
	PlayerIntent Intent,
	string Id
);

