using System.Linq;
using Godot;

public partial class Weapon : Node2D
{
	private Node2D[] TouchedBody;
	public Polygon2D AttackArea;
	public Sprite2D Sword;
	public override void _Ready()
	{
		AttackArea = GetNode<Polygon2D>("AttackArea");
		Sword = GetNode<Sprite2D>("Sword");
		GetNode<CollisionPolygon2D>("Area2D/CollisionPolygon2D").Polygon = AttackArea.Polygon;
	}
	public void OnBodyEntered(Node2D body)//<-
	{
		if (body.HasMethod("OnHit"))
		{
			body.AddToGroup($"TouchedBodyBy{Name}");
		}
	}
	public void OnBodyExited(Node2D body)//<-
	{
		if (body.IsInGroup($"TouchedBodyBy{Name}"))
		{
			body.RemoveFromGroup($"TouchedBodyBy{Name}");
		}
	}
	public void ShortAttck()
	{
		foreach (Node2D body in GetTree().GetNodesInGroup($"TouchedBodyBy{Name}").Cast<Node2D>())
		{
			body.Call("OnHit", this, GD.Randf()*100);
		}
	}
	public void LongAttck()
	{
		foreach (Node2D body in GetTree().GetNodesInGroup($"TouchedBodyBy{Name}").Cast<Node2D>())
		{
			body.Call("OnHit", this, GD.Randf()*500);
		}
	}
}
