using Godot;

public partial class FloatingLabel(string text) : Label
{
	public Vector2 InitSpeed = new(0, -100);
	public Vector2 Speed = new(0, -100);
	public Vector2 Acc = new(0, 10f);
	public float Lifetime = 0.5f;
	public override void _Ready()
	{
		Text = text;
		Speed = InitSpeed;
		GetTree().CreateTimer(Lifetime, false).Timeout += QueueFree;
	}
	public override void _PhysicsProcess(double delta)
	{
		//手动抛物线
		Position += Speed * (float)delta;
		Speed += Acc * (float)delta;
	}
}