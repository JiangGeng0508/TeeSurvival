using Godot;
using System;

public partial class MovingPlatform : AnimatableBody2D
{
    [Export] private Vector2[] _points = [];
    [Export] private float _duration = 1.0f;

    private Vector2[] _globalPoints;
    private Line2D _pathLine;
    private Tween _tween;
    public override void _Ready()
    {
        _pathLine = GetNode<Line2D>("PathLine");
        _pathLine.Points = _points;
        _tween = CreateTween();
        _globalPoints = _points;
        for (var i = 0; i < _points.Length; i++)
        {
            _globalPoints[i] = _points[i] + Position;
            _tween.TweenProperty(this,"position", _points[i], _duration);
        }
        _tween.SetLoops(99999);
    }

    public override void _Process(double delta)
    {
        UpdatePathLine();
    }

    private void UpdatePathLine()
    {
        var relativePoints = _points;
        for (var i = 0; i < _points.Length; i++)
        {
            relativePoints[i] = _points[i] - Position;
        }
        _pathLine.Points = relativePoints;
    }
}
