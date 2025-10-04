using Godot;
using System;

public partial class Scarecrows : RigidBody2D, IHitableEntity
{
	[Export] public bool Moveable = true;
	public override void _Ready()
	{
		if (!Moveable)
		{
			Sleeping = true;
			LockRotation = true;
			Freeze = true;
			FreezeMode = FreezeModeEnum.Static;
		}
	}

	public void OnHit(Vector2 direction, float damage)
	{
		if(Moveable) ApplyCentralImpulse(direction * damage * 10f);
		AddChild(new FloatingLabel($"-{damage}"));//显示伤害数值
	}
}
