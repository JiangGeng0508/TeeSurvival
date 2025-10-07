using System;
using Godot;

public partial class Tee : CharacterBody2D
{
	//移动属性
	[Export] private float _speed = 300f;
	[Export] private float _jumpSpeed = 800f;
	[Export] private float _dodgeSpeed = 1000f;
	[Export] private float _gravity = 40f;
	[Export] private Texture2D _teeSkinTexture;
	[Export] private float _longAttackTime = 0.3f;
	[Export] private DodgeModeEnum _dodgeModeEnum = DodgeModeEnum.Horizontal;
	// 抓钩参数
	[Export] private float _hookMaxDistance = 300f;
	[Export] private float _hookRestLength = 200f;
	[Export] private float _hookStiffness = 1f; // 弹簧系数（越大拉力越强）
	[Export] private float _hookDamping = 1f;   // 阻尼（抑制抖动）
	[Export] private float _hookGravityMultiplier = 5f; // 抓钩时的重力倍率
	private enum DodgeModeEnum
	{
		Horizontal,
		Free
	}
	
	private Line2D _cursorLine; //光标指示线
	private Line2D _hookLine;
	private RayCast2D _hookRay;
	private Sprite2D _cursorMarker; //光标指示点
	private Label _stateLabel;    //跟随人物的标签
	private Vector2 _exVelocity = new(0, 0);//受控制速度（不包括重力）
	private Vector2 _gravityAcc = new(0, 0);//重力效果速度
	private TeeSkin _teeSkin; //皮肤节点
	private Node2D _hand; //手节点
	private Weapon _weapon; //武器节点
	private int _maxJumpCount = 1;//最多跳跃的次数（实际为二段跳）
	private int _jumpCount = 0;
	private bool _isJumping = false;//腾空标志，用于控制释放跳跃键时的处理
	private int _maxDodgeCount = 1;   //最多闪避的次数
	private int _dodgeCount = 0;
	private bool _dodgeFinished = true;
	private float _axis = 0f;
	private Vector2 _homePosition = Vector2.Zero;
	private Vector2 _hookGlobalPosition = Vector2.Zero;
	private bool _isHooking = false;
	public override void _Ready()
	{
		_stateLabel = GetNode<Label>("StateLabel");
		_cursorLine = GetNode<Line2D>("CursorLine");
		_cursorMarker = GetNode<Sprite2D>("CursorMarker");
		_teeSkin = GetNode<TeeSkin>("TeeSkin");
		if (_teeSkinTexture != null) _teeSkin.SkinTexture = _teeSkinTexture;
		_hand = GetNode<Node2D>("Hand");
		_weapon = GetNode<Weapon>("Hand/Weapon");
		_hookRay = GetNode<RayCast2D>("HookRay");
		_hookLine = GetNode<Line2D>("HookLine");

		_homePosition = Position;
	}
	public override void _PhysicsProcess(double delta)
	{
		//跳跃状态检测
		if (IsOnFloor())
		{
			//在地面时重置重力效果，并重置跳跃和闪避次数
			_gravityAcc = new Vector2(0, 0);
			_jumpCount = _maxJumpCount;
			if(_dodgeFinished) _dodgeCount = _maxDodgeCount;
			
		}
		else
		{
			//腾空达到最高点时修复抛物线曲线
			if (_isJumping && Mathf.Abs(Velocity.Y) <= _gravity)
			{
				_gravityAcc = new Vector2(0, 0);
				_exVelocity.Y = 0;
				_isJumping = false;
			}

			//腾空时施加重力加速度
			_gravityAcc += new Vector2(0, _gravity);
			
		}
		//贴墙时重置跳跃次数，实现登墙跳
		if (IsOnWall())
		{
			_jumpCount = _maxJumpCount;
			if (Velocity.Y > 20f)
			{
				_gravityAcc -= new Vector2(0, _gravity * 0.9f);
			} 
		}

		var move = new Vector2(_axis * _speed, 0);
		var hookForce = Vector2.Zero;
		// 更新抓钩射线长度
		_hookRay.TargetPosition = GetLocalMousePosition().Normalized() * _hookMaxDistance;
		
		if (_isHooking)
		{
			// 抓钩力：弹簧 + 阻尼（沿绳方向）
			var toHook = _hookGlobalPosition - GlobalPosition;
			var distance = toHook.Length();
			if (distance > 0.001f)
			{
				var dir = toHook.Normalized();
				var stretch = Mathf.Max(0f, distance - _hookRestLength);
				var springForce = dir * (stretch * _hookStiffness);
				var velAlong = Velocity.Dot(dir);
				var dampingForce = -dir * (velAlong * _hookDamping);
				hookForce = springForce + dampingForce;
			}
			// 抓钩时适当增加重力，避免过度漂浮
			_gravityAcc = new Vector2(0, _gravity * _hookGravityMultiplier);
			_hookLine.Points = [Vector2.Zero, _hookGlobalPosition - GlobalPosition];
		}
		//更新速度
		Velocity = move + _exVelocity + _gravityAcc + hookForce;
		MoveAndSlide();
		
		if(Velocity.Y > 1000f) Velocity = new Vector2(Velocity.X,1000f);
		if (Position.Y > 2000f) Respawn();//掉出地图时回到起始点
		
		_stateLabel.Text = $"";//更新状态标签
		
		//更新光标
		if (_hookRay.IsColliding())
		{
			_cursorLine.DefaultColor = Colors.Red;
			_cursorLine.Points = [Vector2.Zero, _hookRay.GetCollisionPoint() - Position];
		}
		else
		{
			_cursorLine.DefaultColor = Colors.Green;
			_cursorLine.Points = [Vector2.Zero, GetLocalMousePosition().Normalized() * 300f];
		}
		_cursorMarker.Position = _hookGlobalPosition - GlobalPosition;
		// _cursorMarker.Position = GetLocalMousePosition();
		//更新手节点位置和角度
		_hand.Position = GetLocalMousePosition().Normalized() * 20f;
		_hand.Rotation = GetLocalMousePosition().Angle();
	}
	public override void _Input(InputEvent @event)
	{
		_axis = Input.GetAxis("move_left", "move_right");
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.IsActionPressed("dodge"))
			{
				if(_dodgeCount < 0) return;
				switch (_dodgeModeEnum)
				{
					case DodgeModeEnum.Horizontal:
					{
						//按下“闪避”键时，改变X方向速度，并在0.1秒后复原
						var acc = Velocity.X > 0 ? _dodgeSpeed : -_dodgeSpeed;
						_exVelocity.X += acc;
						_dodgeFinished = false;
						GetTree().CreateTimer(0.1f, false).Timeout += () =>
						{
							_exVelocity.X -= acc;
							_dodgeFinished = true;
						};
						break;
					}
					case DodgeModeEnum.Free:
						var dodgeVelocity = GetLocalMousePosition().Normalized() * _dodgeSpeed;
						_exVelocity += dodgeVelocity;
						_dodgeFinished = false;
						GetTree().CreateTimer(0.1f, false).Timeout += () =>
						{
							_exVelocity -= dodgeVelocity;
							_dodgeFinished = true;
						};
						break;
					default:
						break;
				}
				_dodgeCount--;
			}

			if (eventKey.IsActionPressed("respawn"))
				Respawn(eventKey.IsCtrlPressed());
			if (eventKey.IsActionPressed("jump") && _jumpCount > 0)
			{
				//按下“跳跃”键时，改变Y方向速度，重置重力并减少跳跃次数，腾空标志改为true
				_exVelocity += new Vector2(0, -_jumpSpeed);
				_gravityAcc = new Vector2(0, 0);
				_jumpCount--;
				_isJumping = true;
			}
			else if (eventKey.IsActionReleased("jump") && _isJumping)
			{
				//腾空并松开“跳跃”键时，重置Y方向速度和重力，腾空标志改为false
				_exVelocity.Y = 100;
				_isJumping = false;
				_gravityAcc = new Vector2(0, 0);
			}
			if(eventKey.IsActionPressed("indicate_line")) 
				_cursorLine.Show();
			else if(eventKey.IsActionReleased("indicate_line"))
				_cursorLine.Hide();
		}
		if (@event.IsActionPressed("hook"))
		{
			// 发射抓钩：若命中则锁定点并进入抓钩；否则退出
			if (!_hookRay.IsColliding())
			{
				_isHooking = false;
				_hookLine.Hide();
				return;
			}
			var pos = _hookRay.GetCollisionPoint();
			_hookGlobalPosition = pos;
			_isHooking = true;
			_hookLine.Show();
		}
		else if (@event.IsActionReleased("hook"))
		{
			// 取消抓钩
			_isHooking = false;
			_hookLine.Hide();
		}
	}
	private void Respawn(bool reload = false)
	{
		Position = _homePosition;
		if (reload) GetTree().ReloadCurrentScene();
	}
}
