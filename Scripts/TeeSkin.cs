using Godot;
using System;

public partial class TeeSkin : Node2D
{
	[Export]
	public Texture2D SkinTexture;
	private Rect2I _bodyRect = new(0, 0, 64, 64);
	private readonly Rect2I[] _eyesRect = [new Rect2I(0, 64, 64, 64), new Rect2I(64, 64, 64, 64), new Rect2I(128, 64, 64, 64)];
	private readonly Rect2I[] _footLRect = [new Rect2I(0, 128, 64, 64), new Rect2I(64, 128, 64, 64), new Rect2I(128, 128, 64, 64)];
	private readonly Rect2I[] _footRRect = [new Rect2I(0, 192, 64, 64), new Rect2I(64, 192, 64, 64), new Rect2I(128, 192, 64, 64)];
	private AnimatedSprite2D _bodySprite;
	private AnimatedSprite2D _eyesSprite;
	private AnimatedSprite2D _footLSprite;
	private AnimatedSprite2D _footRSprite;
	public override void _Ready()
	{
		if (SkinTexture is null)
		{
			GD.PrintErr("Skin texture is not set.");
			return;
		}
		_bodySprite = new AnimatedSprite2D { SpriteFrames = CutImageAnimation(SkinTexture, [_bodyRect]) };
		_eyesSprite = new AnimatedSprite2D { SpriteFrames = CutImageAnimation(SkinTexture, _eyesRect) };
		_footLSprite = new AnimatedSprite2D { SpriteFrames = CutImageAnimation(SkinTexture, _footLRect) };
		_footRSprite = new AnimatedSprite2D { SpriteFrames = CutImageAnimation(SkinTexture, _footRRect) };
		AddChild(_bodySprite);
		AddChild(_eyesSprite);
		AddChild(_footLSprite);
		AddChild(_footRSprite);
		
		PlayFootAnim();
	}
	private static SpriteFrames CutImageAnimation(Texture2D texture, Rect2I[] rects)
	{
		var spriteFrames = new SpriteFrames();
		foreach (var t in rects)
		{
			//剪切纹理(Texture2D)并添加为帧(SpriteFrame)
			var image = texture.GetImage();
			var cutImage = image.GetRegion(t);
			spriteFrames.AddFrame("default", ImageTexture.CreateFromImage(cutImage));
		}
		return spriteFrames;
	}

	public override void _Process(double delta)
	{
		var mousePos = GetLocalMousePosition();
		if (mousePos.Length() > 32f)
			_eyesSprite.Position = mousePos.Normalized() * 3f;
		else 
			_eyesSprite.Position = mousePos * 3f / 32f;
	}

	private void PlayFootAnim(bool play = true)
	{
		if(play)
		{
			_footLSprite.Play();
			_footRSprite.Play();
		}
		else
		{
			_footLSprite.Stop();
			_footRSprite.Stop();
		}
	}
}
