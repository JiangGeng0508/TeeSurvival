using Godot;
using System;

public partial class Scarecrows : RigidBody2D, IHitableEntity
{
	public void OnHit(Node2D sender, float damage)
	{
		GD.Print($"{Name} Hitted by {sender.Name}");
		AddChild(new FloatingLabel($"-{damage}"));
	}
}
