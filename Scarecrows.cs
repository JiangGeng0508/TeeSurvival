using Godot;
using System;

public partial class Scarecrows : RigidBody2D, IHitableEntity
{
	public void OnHit(Node2D sender, float damage)
	{
		GD.Print($"{Name} Hitted by {sender.Name}");
		var damagelabel = new Label
		{
			Text = $"-{damage}",
			Position = new Vector2(0, -30)
		};
		AddChild(damagelabel);
		GetTree().CreateTimer(1.0f).Timeout += () =>
		{
			damagelabel.QueueFree();
		};
	}
}
