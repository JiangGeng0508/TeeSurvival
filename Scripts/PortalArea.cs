using Godot;
using System;

public partial class PortalArea : Area2D
{
    [Export] public PackedScene TargetScene;

    public void OnSomethingEntered(Node2D node)
    {
        if (TargetScene is null)
        {
            GD.PrintErr("TargetScene is null,can't change scene");
            return;
        }
        GetTree().ChangeSceneToPacked(TargetScene);
    }
}
