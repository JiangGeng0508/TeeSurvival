using System.Linq;
using Godot;

public partial class Weapon : Node2D
{
	private Node2D[] TouchedBody;
	public Polygon2D AttackArea;
	public Sprite2D Sword;
	private float _PressTime = 0; //按下按键的时间
	private bool _IsPressing = false; //是否正在按下按键
	public float LongAttckTime = 0.5f;
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
			body.Call("OnHit", (GetGlobalMousePosition() - GlobalPosition).Normalized(), GD.Randf() * 20 + 20);
		}
	}
	public void LongAttck()
	{
		foreach (Node2D body in GetTree().GetNodesInGroup($"TouchedBodyBy{Name}").Cast<Node2D>())
		{
			body.Call("OnHit", (GetGlobalMousePosition() - GlobalPosition).Normalized(), GD.Randf() * 100 + 100);
		}
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouse eventMouse)
		{
			if (@event.IsActionPressed("attack"))
			{
				//按下攻击时重置按键时间并将按键标识置为true
				_PressTime = 0;
				_IsPressing = true;
			}
			if (@event.IsActionReleased("attack"))
			{
				//松开攻击时将按键标识置为false
				_IsPressing = false;
				if (_PressTime < LongAttckTime)//长按时间超过LongAttckTime
				{
					//短攻击
					ShortAttck();
				}
				else
				{
					//长攻击
					LongAttck();
				}
				_PressTime = 0;//修复长按表现
			}
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		//按键按下时间记录
		if (_IsPressing)
		{
			_PressTime += (float)delta;
		}
	}
}
