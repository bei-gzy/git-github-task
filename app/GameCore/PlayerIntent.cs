namespace App.GameCore;

public readonly record struct Direction(DirectionType Dir);
public readonly record struct Skills(SkillsType Skill);

public readonly union PlayerIntent(Direction, Skills);

public interface IInputSource//这个端口没毛
{
	PlayerIntent Poll();
}

// namespace App.Models.Inputs;

// public record UserInput;

// public record Move(
// 	State.Direction? Direction
// ) : UserInput;
// record Sprint : UserInput
