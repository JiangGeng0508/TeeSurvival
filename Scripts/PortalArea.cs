using Godot;
using System;

public partial class PortalArea : Area2D
{
    [Export] public PackedScene TargetScene;

    public void OnSomethingEntered(Node2D node)
    {
        if(node is not Tee) return;
        if (TargetScene is null)
        {
            GD.PrintErr("TargetScene is null,can't change scene");
            return;
        }

        CallDeferred(nameof(ChangeToTargetScene));
    }
    private void ChangeToTargetScene() => GetTree().ChangeSceneToPacked(TargetScene);
}
