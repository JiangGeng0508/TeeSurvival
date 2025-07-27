using Godot;
using System;

public partial class Tee : CharacterBody2D
{
	[Export]
	public float Speed = 300;
	[Export]
	public float JumpSpeed = 800;
	[Export]
	public float DodgeSpeed = 1000;
	[Export]
	public float Gravity = 40;
	public Label StateLabel = new();
	private Vector2 _velocity = new(0, 0);
	private Vector2 gravityAcc = new(0, 0);
	public int MaxJumpCount = 1;
	private int JumpCount = 0;
	private bool IsJumping = false;
	public int MaxDodgeCount = 1;
	private int DodgeCount = 0;
	public override void _Ready()
	{
		AddChild(StateLabel);
	}
	public override void _PhysicsProcess(double delta)
	{
		if (IsOnFloor())
		{
			gravityAcc = new Vector2(0, 0);
			JumpCount = MaxJumpCount;
			DodgeCount = MaxDodgeCount;
		}
		else
		{
			if (IsJumping && Velocity.Y >= -Gravity / 2 && Velocity.Y <= Gravity / 2)
			{
				gravityAcc = new Vector2(0, 0);
				_velocity.Y = 0;
				IsJumping = false;
			}
			gravityAcc += new Vector2(0, Gravity);
		}
		if (IsOnWall())
		{
			JumpCount = MaxJumpCount;
		}

		Velocity = _velocity + gravityAcc;
		MoveAndSlide();

		StateLabel.Text = $"{Velocity}";

		if (Position.Y > 2000)
		{
			Position = new Vector2(415, 376);
		}
	}
	public override void _ShortcutInput(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed)
			{
				if (eventKey.IsActionPressed("move_left"))
				{
					_velocity += new Vector2(-Speed, 0);
				}
				if (eventKey.IsActionPressed("move_right"))
				{
					_velocity += new Vector2(Speed, 0);
				}

				if (eventKey.IsActionPressed("jump") && JumpCount > 0)
				{
					_velocity += new Vector2(0, -JumpSpeed);
					gravityAcc = new Vector2(0, 0);
					JumpCount--;
					IsJumping = true;
				}
				if (eventKey.IsActionPressed("dodge"))
				{
					var acc = _velocity.X > 0 ? DodgeSpeed : -DodgeSpeed;
					_velocity.X += acc;
					GetTree().CreateTimer(0.1f, false).Timeout += () =>
					{
						_velocity.X -= acc;
					};
				}
			}
			else
			{
				if (eventKey.IsActionReleased("move_left"))
				{
					_velocity += new Vector2(Speed, 0);
				}
				if (eventKey.IsActionReleased("move_right"))
				{
					_velocity += new Vector2(-Speed, 0);
				}
				if (eventKey.IsActionReleased("jump") && IsJumping)
				{
					_velocity.Y = 100;
					IsJumping = false;
					if (Velocity.Y < 0)
					{
						gravityAcc = new Vector2(0, 0);
					}
				}
			}
		}
	}
}
