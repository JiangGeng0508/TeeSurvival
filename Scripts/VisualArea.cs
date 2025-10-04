using Godot;
using System;
[Tool]
public partial class VisualArea : Area2D
{
    [Export(PropertyHint.Enum)] private VisualAreaModeEnum _visualAreaModeEnum = VisualAreaModeEnum.OnlyContact;
    [Export] public float KeepTime = 1f;
    
    [ExportGroup("Color")]
    [Export] public bool ColorVisible = true;
    [Export] public Color NormalColor = Colors.White;
    [Export] public Color HighlightColor = Colors.Red;
    
    [ExportGroup("Text")]
    [Export] public string LabelText = "";

    private enum VisualAreaModeEnum
    {
        OnlyContact,
        KeepTime,
        KeepForever
    }

    private ColorRect _colorRect;
    private RichTextLabel _richTextLabel;
    public override void _Ready()
    {
        _colorRect = GetNode<ColorRect>("ColorRect");
        _richTextLabel = GetNode<RichTextLabel>("RichTextLabel");
        
        _colorRect.Color = NormalColor;
        _colorRect.Visible = ColorVisible;
        _richTextLabel.Text = LabelText;
    }
    private void OnBodyEntered(Node2D node)
    {
        if(node is not Tee) return;
        _colorRect.Color = HighlightColor;
        _richTextLabel.Show();
    }
    private void OnBodyExited(Node2D node)
    {
        if (node is not Tee) return;
        switch (_visualAreaModeEnum)
        {
            case VisualAreaModeEnum.OnlyContact:
                _colorRect.Color = NormalColor; 
                _richTextLabel.Hide();
                break;
            case VisualAreaModeEnum.KeepTime:
                GetTree().CreateTimer(KeepTime, false).Timeout += () =>
                {
                    _colorRect.Color = NormalColor; 
                    _richTextLabel.Hide();
                };
                break;
            case VisualAreaModeEnum.KeepForever:
            default:
                break;
        }
    }
}
