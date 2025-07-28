using Godot;

public partial class FloatingLabel(string text) : Label
{
	public Vector2 InitSpeed = new(0, -10);
	public Vector2 Speed = new(0, -10);
	public Vector2 Acc = new(0, 0.5f);
	public float Lifetime = 0.5f;
	public override void _Ready()
	{
		Text = text;
		Speed = InitSpeed;
		GetTree().CreateTimer(Lifetime, false).Timeout += QueueFree;
	}
	public override void _PhysicsProcess(double delta)
	{
		Position += Speed * (float)delta;
		Speed += Acc * (float)delta;
	}
}