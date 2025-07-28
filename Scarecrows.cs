using Godot;
using System;

public partial class Scarecrows : RigidBody2D, IHitableEntity
{
	public void OnHit(Vector2 direction, float damage)
	{
		ApplyCentralImpulse(direction * damage * 10f);
		AddChild(new FloatingLabel($"-{damage}"));
	}
}
