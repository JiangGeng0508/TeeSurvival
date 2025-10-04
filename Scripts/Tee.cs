using Godot;

public partial class Tee : CharacterBody2D
{
	//移动属性
	[Export] public float Speed = 300f;
	[Export] public float JumpSpeed = 800f;
	[Export] public float DodgeSpeed = 1000f;
	[Export] public float Gravity = 40f;
	[Export] public Texture2D TeeSkinTexture;
	[Export] public float LongAttackTime = 0.3f;
	
	private Line2D _cursorLine; //光标指示线
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
	private float axis = 0f;
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
			if (_isJumping && Mathf.Abs(Velocity.Y) <= Gravity)
			{
				_gravityAcc = new Vector2(0, 0);
				_exVelocity.Y = 0;
				_isJumping = false;
				GD.Print("跳跃结束");
			}

			//腾空时施加重力加速度
			_gravityAcc += new Vector2(0, Gravity);
			
		}
		//贴墙时重置跳跃次数，实现登墙跳
		if (IsOnWall())
		{
			_jumpCount = _maxJumpCount;
			if (Velocity.Y > 20f)
			{
				_gravityAcc -= new Vector2(0, Gravity * 0.9f);
			} 
		}

		var move = new Vector2(axis * Speed, 0);
		//更新速度
		Velocity = move + _exVelocity + _gravityAcc;
		if(Velocity.Y > 1000f) Velocity = new Vector2(Velocity.X,1000f);
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
		axis = Input.GetAxis("move_left", "move_right");
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed)
			{
				if (eventKey.IsActionPressed("jump") && _jumpCount > 0)
				{
					//按下“跳跃”键时，改变Y方向速度，重置重力并减少跳跃次数，腾空标志改为true
					_exVelocity += new Vector2(0, -JumpSpeed);
					_gravityAcc = new Vector2(0, 0);
					_jumpCount--;
					_isJumping = true;
				}
				if (eventKey.IsActionPressed("dodge") && _dodgeCount > 0 && _exVelocity.X != 0)
				{
					//按下“闪避”键时，改变X方向速度，并在0.1秒后复原
					var acc = _exVelocity.X > 0 ? DodgeSpeed : -DodgeSpeed;
					_exVelocity.X += acc;
					GetTree().CreateTimer(0.1f, false).Timeout += () =>
					{
						_exVelocity.X -= acc;
					};
					_dodgeCount--;
				}
			}
			else
			{
				if (eventKey.IsActionReleased("jump") && _isJumping)
				{
					//腾空并松开“跳跃”键时，重置Y方向速度和重力，腾空标志改为false
					_exVelocity.Y = 100;
					_isJumping = false;
					_gravityAcc = new Vector2(0, 0);
				}
			}
			SetFoot(!(Mathf.Abs(_exVelocity.X) > 10));
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
