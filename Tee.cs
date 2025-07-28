using Godot;
using System;

public partial class Tee : CharacterBody2D
{
	//移动属性
	[Export]
	public float Speed = 300;
	[Export]
	public float JumpSpeed = 800;
	[Export]
	public float DodgeSpeed = 1000;
	[Export]
	public float Gravity = 40;
	[Export]
	public Texture2D teeSkinTexture;
	[Export]
	public float LongAttckTime = 0.3f;
	public Line2D CursorLine; //光标指示线
	public Sprite2D CursorMarker; //光标指示点
	public Label StateLabel;    //跟随人物的标签
	private Vector2 _velocity = new(0, 0);//受控制速度（不包括重力）
	private Vector2 gravityAcc = new(0, 0);//重力效果速度
	public TeeSkin teeSkin; //皮肤节点
	public Node2D Hand; //手节点
	public Weapon weapon; //武器节点
	private float _PressTime = 0; //按下按键的时间
	private bool _IsPressing = false; //是否正在按下按键
	public int MaxJumpCount = 1;//最多跳跃的次数（实际为二段跳）
	private int JumpCount = 0;
	private bool IsJumping = false;//腾空标志，用于控制释放跳跃键时的处理
	public int MaxDodgeCount = 1;   //最多闪避的次数
	private int DodgeCount = 0;
	public override void _Ready()
	{
		StateLabel = GetNode<Label>("StateLabel");
		CursorLine = GetNode<Line2D>("CursorLine");
		CursorMarker = GetNode<Sprite2D>("CursorMarker");
		teeSkin = GetNode<TeeSkin>("TeeSkin");
		teeSkin.skinTexture = teeSkinTexture is null ? teeSkin.skinTexture : teeSkinTexture;
		Hand = GetNode<Node2D>("Hand");
		weapon = GetNode<Weapon>("Hand/Weapon");
	}
	public override void _PhysicsProcess(double delta)
	{
		//跳跃状态检测
		if (IsOnFloor())
		{
			//在地面时重置重力效果，并重置跳跃和闪避次数
			gravityAcc = new Vector2(0, 0);
			JumpCount = MaxJumpCount;
			DodgeCount = MaxDodgeCount;
		}
		else
		{
			//腾空达到最高点时修复抛物线曲线
			if (IsJumping && Velocity.Y >= -Gravity / 2 && Velocity.Y <= Gravity / 2)
			{
				gravityAcc = new Vector2(0, 0);
				_velocity.Y = 0;
				IsJumping = false;
			}
			//腾空时施加重力加速度
			gravityAcc += new Vector2(0, Gravity);
		}
		//贴墙时重置跳跃次数，实现登墙跳
		if (IsOnWall())
		{
			JumpCount = MaxJumpCount;
			if (_velocity.X != 0 && Velocity.Y > 100)
			{
				gravityAcc -= new Vector2(0, Gravity * 0.5f);
			} 
		}
		//更新速度
		Velocity = _velocity + gravityAcc;
		MoveAndSlide();
		//更新状态标签
		StateLabel.Text = $"{_PressTime > LongAttckTime}";
		//调出地图时回到起始点
		if (Position.Y > 2000)
		{
			Position = new Vector2(415, 376);
		}
		//控制眼睛偏移
		teeSkin.EyesSprite.Position = GetLocalMousePosition().Normalized() * 3f;
		//更新光标
		CursorLine.Points = [Vector2.Zero, GetLocalMousePosition()];
		CursorMarker.Position = GetLocalMousePosition();
		//更新手节点位置和角度
		Hand.Position = GetLocalMousePosition().Normalized() * 20f;
		Hand.Rotation = GetLocalMousePosition().Angle();
		//按键按下时间记录
		if (_IsPressing)
		{
			_PressTime += (float)delta;
		}
	}
	public override void _Input(InputEvent @event)
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
					//按下“跳跃”键时，改变Y方向速度，重置重力并减少跳跃次数，腾空标志改为true
					_velocity += new Vector2(0, -JumpSpeed);
					gravityAcc = new Vector2(0, 0);
					JumpCount--;
					IsJumping = true;
				}
				if (eventKey.IsActionPressed("dodge") && DodgeCount > 0 && _velocity.X != 0)
				{
					//按下“闪避”键时，改变X方向速度，并在0.1秒后复原
					var acc = _velocity.X > 0 ? DodgeSpeed : -DodgeSpeed;
					_velocity.X += acc;
					GetTree().CreateTimer(0.1f, false).Timeout += () =>
					{
						_velocity.X -= acc;
					};
					DodgeCount--;
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
					//腾空并松开“跳跃”键时，重置Y方向速度和重力，腾空标志改为false
					_velocity.Y = 100;
					IsJumping = false;
					gravityAcc = new Vector2(0, 0);
				}
			}
			if (_velocity.X > 10 || _velocity.X < -10)//移动时启动动画
			{
				SetFoot(false);
			}
			else
			{
				SetFoot(true);
			}
		}
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
					weapon.ShortAttck();
				}
				else
				{
					//长攻击
					weapon.LongAttck();
				}
				_PressTime = 0;//修复长按表现
			}
		}
	}
	public void SetFoot(bool idle)
	{
		if (idle)
		{
			teeSkin.FootLSprite.Stop();
			teeSkin.FootRSprite.Stop();
			teeSkin.FootLSprite.Frame = 0;
			teeSkin.FootRSprite.Frame = 0;
		}
		else
		{
			teeSkin.FootLSprite.Play();
			teeSkin.FootRSprite.Play();
		}
	}
}
