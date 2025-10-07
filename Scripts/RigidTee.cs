using Godot;
using System;

public partial class RigidTee : RigidBody2D
{
    [Export] private float _moveMaxSpeed = 200f;
    [Export] private float _moveAcc = 50f;
    [Export] private float _jumpAcc = 1000f;
    [Export] private float _jumpImp = 500f;
    private bool _isJumping = false;
    private float _axis = 0f;
    private Vector2 _homePosition = Vector2.Zero;

    public override void _Ready()
    {
        _homePosition = Position;
    }

    public override void _PhysicsProcess(double delta)
    {
        _axis = Input.GetAxis("move_left", "move_right");
        if(Mathf.Abs(_axis) > 0.1f && Mathf.Abs(LinearVelocity.X) < _moveMaxSpeed) LinearVelocity += new Vector2(_axis * _moveAcc, 0);
        
        LinearVelocity -= new Vector2(Mathf.Clamp(LinearVelocity.X * 0.1f, -_moveMaxSpeed, _moveMaxSpeed),0);

        if (Position.Y > 2000f) Position = _homePosition;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.IsActionPressed("jump")) StartJump();
            if (eventKey.IsActionReleased("jump")) EndJump();
        }
    }

    private void OnBodyShapeEntered(Rid bodyRid, Node body, int bodyShapeIndex, int localShapeIndex)
    {
        GD.Print($"bodyRid:{bodyRid},body:{body.Name},bodyId:{bodyShapeIndex},localId:{localShapeIndex}");
    }

    private void OnBodyEntered(Node node)
    {
        GD.Print($"node:{node.Name}");
    }

    private void StartJump()
    {
        GD.Print("start jump");
        if(_isJumping) return;
        LinearVelocity = new Vector2(LinearVelocity.X, 0);
        ApplyCentralImpulse(new Vector2(0, -_jumpImp));
        AddConstantCentralForce(new Vector2(0, -_jumpAcc));
        _isJumping = true;
        GetTree().CreateTimer(0.1f, false, true).Timeout += EndJump;
    }

    private void EndJump()
    {
        GD.Print("end jump");
        if(!_isJumping) return;
        AddConstantCentralForce(new Vector2(0, _jumpAcc));
        _isJumping = false;
    }
}
