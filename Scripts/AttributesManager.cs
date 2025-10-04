using Godot;
using System;
using Godot.Collections;

public partial class AttributesManager : Node
{
    public static float MaxHealth = 100;
    public static float Speed = 300f;

    public static void SaveAttributes()
    {
        var dic = new Dictionary<string, float>();
        dic.Add("MaxHealth",MaxHealth);
    }
    public static void LoadAttributes()
    {
        
    }
}
