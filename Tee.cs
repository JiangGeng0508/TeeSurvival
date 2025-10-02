using Godot;

public partial class Tee : CharacterBody2D
{
	//移动属性
	[Export] public float Speed = 300;
	[Export] public float JumpSpeed = 800;
	[Export] public float DodgeSpeed = 1000;
	[Export] public float Gravity = 40;
	[Export] public float MaxFallSpeed = 400f;
	[Export] public Texture2D TeeSkinTexture;
	[Export] public float LongAttackTime = 0.3f;
	[Export] public float JumpTime = 0.5f;
	
	private Line2D _cursorLine; //光标指示线
	private Sprite2D _cursorMarker; //光标指示点
	private Label _stateLabel;    //跟随人物的标签
	private Vector2 _moveVelocity = new(0, 0);//受控制速度（不包括重力）
	private Vector2 _gravityAcc = new(0, 0);//重力效果速度
	private TeeSkin _teeSkin; //皮肤节点
	private Node2D _hand; //手节点
	private Weapon _weapon; //武器节点
	private int _maxJumpCount = 1;//最多跳跃的次数（实际为二段跳）
	private int _jumpCount = 0;
	private bool _isJumping = false;//腾空标志，用于控制释放跳跃键时的处理
	private float _jumpingTime = 0f;
	private int _maxDodgeCount = 1;   //最多闪避的次数
	private int _dodgeCount = 0;
	public override void _Ready()
	{
		_stateLabel = GetNode<Label>("StateLabel");
		_cursorLine = GetNode<Line2D>("CursorLine");
		_cursorMarker = GetNode<Sprite2D>("CursorMarker");
		_teeSkin = GetNode<TeeSkin>("TeeSkin");
		if (TeeSkinTexture != null) _teeSkin.skinTexture = TeeSkinTexture;
		_hand = GetNode<Node2D>("Hand");
		_weapon = GetNode<Weapon>("Hand/Weapon");
	}
	public override void _PhysicsProcess(double delta)
	{
		//跳跃状态检测
		if (IsOnFloor())
		{
			//在地面时重置重力效果，并重置跳跃和闪避次数
			_gravityAcc = new Vector2(0, 0);
			_jumpCount = _maxJumpCount;
			_dodgeCount = _maxDodgeCount;
		}
		else
		{
			//腾空达到最高点时修复抛物线曲线
			if (_isJumping && (Mathf.Abs(Velocity.Y) <= Gravity / 2 || _jumpingTime <= 0f))
			{
				_gravityAcc = new Vector2(0, 0);
				_moveVelocity.Y = 0;
				_isJumping = false;
				GD.Print("跳跃结束");
			}

			_jumpingTime -= (float)delta;
			GD.Print(_jumpingTime);
			//腾空时施加重力加速度
			_gravityAcc += new Vector2(0, Gravity);
			_gravityAcc.Y = Mathf.Clamp(_gravityAcc.Y, -MaxFallSpeed, MaxFallSpeed);
			
		}
		//贴墙时重置跳跃次数，实现登墙跳
		if (IsOnWall())
		{
			_jumpCount = _maxJumpCount;
			if (_moveVelocity.X != 0 && Velocity.Y > 20)
			{
				_gravityAcc -= new Vector2(0, Gravity * 0.9f);
			} 
		}
		//更新速度
		Velocity = _moveVelocity + _gravityAcc;
		MoveAndSlide();
		//更新状态标签
		_stateLabel.Text = $"";
		//调出地图时回到起始点
		if (Position.Y > 2000)
		{
			Position = new Vector2(415, 376);
		}
		//控制眼睛偏移
		_teeSkin.EyesSprite.Position = GetLocalMousePosition().Normalized() * 3f;
		//更新光标
		_cursorLine.Points = [Vector2.Zero, GetLocalMousePosition()];
		_cursorMarker.Position = GetLocalMousePosition();
		//更新手节点位置和角度
		// Hand.Position = GetLocalMousePosition().Normalized() * 20f;
		_hand.Rotation = GetLocalMousePosition().Angle();
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed)
			{
				if (eventKey.IsActionPressed("move_left"))
				{
					_moveVelocity += new Vector2(-Speed, 0);
				}
				if (eventKey.IsActionPressed("move_right"))
				{
					_moveVelocity += new Vector2(Speed, 0);
				}

				if (eventKey.IsActionPressed("jump") && _jumpCount > 0)
				{
					//按下“跳跃”键时，改变Y方向速度，重置重力并减少跳跃次数，腾空标志改为true
					_moveVelocity += new Vector2(0, -JumpSpeed);
					_gravityAcc = new Vector2(0, 0);
					_jumpCount--;
					_isJumping = true;
					_jumpingTime = JumpTime;
				}
				if (eventKey.IsActionPressed("dodge") && _dodgeCount > 0 && _moveVelocity.X != 0)
				{
					//按下“闪避”键时，改变X方向速度，并在0.1秒后复原
					var acc = _moveVelocity.X > 0 ? DodgeSpeed : -DodgeSpeed;
					_moveVelocity.X += acc;
					GetTree().CreateTimer(0.1f, false).Timeout += () =>
					{
						_moveVelocity.X -= acc;
					};
					_dodgeCount--;
				}
			}
			else
			{
				if (eventKey.IsActionReleased("move_left"))
				{
					_moveVelocity += new Vector2(Speed, 0);
				}
				if (eventKey.IsActionReleased("move_right"))
				{
					_moveVelocity += new Vector2(-Speed, 0);
				}
				if (eventKey.IsActionReleased("jump") && _isJumping)
				{
					//腾空并松开“跳跃”键时，重置Y方向速度和重力，腾空标志改为false
					_moveVelocity.Y = 100;
					_isJumping = false;
					_gravityAcc = new Vector2(0, 0);
					_jumpingTime = 0f;
				}
			}
			if (_moveVelocity.X > 10 || _moveVelocity.X < -10)//移动时启动动画
			{
				SetFoot(false);
			}
			else
			{
				SetFoot(true);
			}
		}
	}
	public void SetFoot(bool idle)
	{
		if (idle)
		{
			_teeSkin.FootLSprite.Stop();
			_teeSkin.FootRSprite.Stop();
			_teeSkin.FootLSprite.Frame = 0;
			_teeSkin.FootRSprite.Frame = 0;
		}
		else
		{
			_teeSkin.FootLSprite.Play();
			_teeSkin.FootRSprite.Play();
		}
	}
}
