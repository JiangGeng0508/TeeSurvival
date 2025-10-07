using Godot;
using System;
using Godot.Collections;

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
        Trigger
    }

    private ColorRect _colorRect;
    private RichTextLabel _richTextLabel;
    private Timer _timer;
    public override void _ValidateProperty(Dictionary property)
    {
        _Ready();
        _richTextLabel.Show();
    }

    public override void _Ready()
    {
        _colorRect = GetNode<ColorRect>("ColorRect");
        _richTextLabel = GetNode<RichTextLabel>("RichTextLabel");
        _colorRect.Color = NormalColor;
        _colorRect.Visible = ColorVisible;
        _richTextLabel.Text = LabelText;
        _richTextLabel.Hide();
        if (_visualAreaModeEnum != VisualAreaModeEnum.KeepTime) return;
        _timer = new Timer
        {
            WaitTime = KeepTime,
            OneShot = true,
            Autostart = false
        };
        AddChild(_timer);
        _timer.Timeout += TurnOff;
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
                TurnOff();
                break;
            case VisualAreaModeEnum.KeepTime:
                _timer.Start();
                break;
            case VisualAreaModeEnum.Trigger:
            default:
                break;
        }
    }

    private void TurnOff()
    {
        _colorRect.Color = NormalColor; 
        _richTextLabel.Hide();
    }
}
