using Godot;
using System;

public partial class Enemy : CharacterBody2D,IHitableEntity
{
	[Export(PropertyHint.Enum, "float,idle,wander")]
	public string MotionType = "float";
	//移动属性
	[Export]
	public float Speed = 300;
	[Export]
	public float Gravity = 40;
	private bool _wanderDirection = true;
	private Vector2 _velocity = new(0, 0);//受控制速度（不包括重力）
	private Vector2 _motion = new(0, 0);
	private Vector2 gravityAcc = new(0, 0);//重力效果速度

	public override void _PhysicsProcess(double delta)
	{
		switch (MotionType)
		{
			case "float":
				break;
			case "idle":
				_motion = Vector2.Zero;
				break;
			case "wander":
				if (IsOnWall())
				{
					_wanderDirection =!_wanderDirection;
				}
				_motion = _wanderDirection? new Vector2(Speed, 0) : new Vector2(-Speed, 0);
				break;
			default:
				break;
		}
		//重力效果
		if (IsOnFloor())
		{
			gravityAcc = new Vector2(0, 0);
		}
		else
		{
			gravityAcc += new Vector2(0, Gravity);
		}
		if (_velocity.LengthSquared() > 10f)
		{
			_velocity /= 1.1f;
		}
		else
		{
			_velocity = Vector2.Zero;
		}
		//更新速度
		Velocity = _velocity + gravityAcc + _motion;
		MoveAndSlide();
	}

	public void OnHit(Vector2 direction, float damage)
	{
		GD.Print($"{direction}");
		_velocity += direction.Normalized() * damage * 10f;
	}
}
