using Godot;
using System;

public partial class TeeSkin : Node2D
{
	[Export]
	public Texture2D skinTexture;
	//纹理区域格式
	public Rect2I BodyRect = new(0, 0, 64, 64);
	public Rect2I[] EyesRect = [new Rect2I(0, 64, 64, 64), new Rect2I(64, 64, 64, 64), new Rect2I(128, 64, 64, 64)];
	public Rect2I[] FootLRect = [new Rect2I(0, 128, 64, 64), new Rect2I(64, 128, 64, 64), new Rect2I(128, 128, 64, 64)];
	public Rect2I[] FootRRect = [new Rect2I(0, 192, 64, 64), new Rect2I(64, 192, 64, 64), new Rect2I(128, 192, 64, 64)];
	public AnimatedSprite2D BodySprite;
	public AnimatedSprite2D EyesSprite;
	public AnimatedSprite2D FootLSprite;
	public AnimatedSprite2D FootRSprite;
	public override void _Ready()
	{
		if (skinTexture is null)
		{
			GD.PrintErr("Skin texture is not set.");
			return;
		}
		BodySprite = new() { SpriteFrames = CutImageAnimation(skinTexture, [BodyRect]) };
		EyesSprite = new() { SpriteFrames = CutImageAnimation(skinTexture, EyesRect) };
		FootLSprite = new() { SpriteFrames = CutImageAnimation(skinTexture, FootLRect) };
		FootRSprite = new() { SpriteFrames = CutImageAnimation(skinTexture, FootRRect) };
		AddChild(BodySprite);
		AddChild(EyesSprite);
		AddChild(FootLSprite);
		AddChild(FootRSprite);
	}
	public static SpriteFrames CutImageAnimation(Texture2D texture, Rect2I[] rects)
	{
		var spriteframes = new SpriteFrames();
		for (int i = 0; i < rects.Length; i++)
		{
			//剪切纹理(Texture2D)并添加为帧(SpriteFrame)
			var image = texture.GetImage();
			var cutImage = image.GetRegion(rects[i]);
			spriteframes.AddFrame("default", ImageTexture.CreateFromImage(cutImage));
		}
		return spriteframes;
	}
}
