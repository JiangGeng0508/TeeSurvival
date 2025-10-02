using System.Linq;
using Godot;

public partial class Weapon : Node2D
{
	[Signal]
	public delegate void PressTimeEventHandler(float percentage);
	
	private Node2D[] _touchedBody;
	private Polygon2D _attackArea;
	private CollisionPolygon2D _weaponCollision;
	private Tween _tween;
	private float _pressTime = 0; //按下按键的时间
	private bool _isPressing = false; //是否正在按下按键
	private float _longAttackTime = 0.5f;
	public override void _Ready()
	{
		_attackArea = GetNode<Polygon2D>("AttackArea");
		_weaponCollision = GetNode<CollisionPolygon2D>("Area2D/CollisionPolygon2D");
		// GetNode<CollisionPolygon2D>("Area2D/CollisionPolygon2D").Polygon = _attackArea.Polygon;
		
		// tween = CreateTween();
		// tween.TweenProperty(Sword, "rotation", -Mathf.Pi / 4, 0.1f);
		// tween.TweenProperty(Sword, "rotation", Mathf.Pi / 4, 0.3f);
		// tween.TweenProperty(Sword, "rotation", 0, 0.1f);
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
	private void ShortAttack()
	{
		foreach (var body in GetTree().GetNodesInGroup($"TouchedBodyBy{Name}").Cast<Node2D>())
		{
			body.Call("OnHit", (GetGlobalMousePosition() - GlobalPosition).Normalized(), GD.Randf() * 20 + 20);
		}
		// tween = CreateTween();
		// tween.TweenProperty(Sword, "rotation", -Mathf.Pi / 4, 0.1f);
		// tween.TweenProperty(Sword, "rotation", Mathf.Pi / 4, 0.3f);
		// tween.TweenProperty(Sword, "rotation", 0, 0.1f);
	}
	private void LongAttack()
	{
		foreach (var body in GetTree().GetNodesInGroup($"TouchedBodyBy{Name}").Cast<Node2D>())
		{
			body.Call("OnHit", (GetGlobalMousePosition() - GlobalPosition).Normalized(), GD.Randf() * 100 + 100);
		}
		// tween = CreateTween();
		// tween.TweenProperty(Sword, "rotation", -Mathf.Pi / 4, 0.1f);
		// tween.TweenProperty(Sword, "rotation", Mathf.Pi / 4, 0.3f);
		// tween.TweenProperty(Sword, "rotation", 0, 0.1f);
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouse eventMouse)
		{
			if (@event.IsActionPressed("attack"))
			{
				//按下攻击时重置按键时间并将按键标识置为true
				_pressTime = 0;
				_isPressing = true;
			}
			if (@event.IsActionReleased("attack"))
			{
				//松开攻击时将按键标识置为false
				_isPressing = false;
				if (_pressTime < _longAttackTime)//长按时间超过LongAttckTime
				{
					//短攻击
					ShortAttack();
				}
				else
				{
					//长攻击
					LongAttack();
				}
				_pressTime = 0;//修复长按表现
			}
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		//按键按下时间记录
		if (_isPressing)
		{
			_pressTime += (float)delta;
		}

		var percentage = Mathf.Clamp(_pressTime / _longAttackTime,0f,1f);
		EmitSignalPressTime(percentage);
		_weaponCollision.Scale = new Vector2(1f + percentage, 1f + percentage);
	}
}
