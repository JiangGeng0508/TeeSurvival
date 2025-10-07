using Godot;
using System;
using Godot.Collections;

[Tool]
public partial class ShootingCone : ColorRect
{
    private ShaderMaterial _material;

    [Export(PropertyHint.Range,"0f,360f")] public float ConeAngle = 90f;
    [Export(PropertyHint.Range,"0f,360f")] public float StartAngle = 0.0f;
    [Export(PropertyHint.Range,"0f,4f")] public float FadeIntensity = 3.0f;
    [Export(PropertyHint.Range,"0f,1f")] public float CircleRadius = 0.5f;
    [Export(PropertyHint.Range,"0f,1f")] public float InnerCircleAlpha = 0.8f;
    [Export(PropertyHint.Range,"0f,0.1f")] public float OutlineThickness = 0.015f;
    [Export] public Vector4 CustomColor = new(1.0f, 1.0f, 1.0f, 1.0f);
    [Export] public float ConeScale = 1.0f;

    public override void _ValidateProperty(Dictionary property)
    {
        _Ready();
    }

    public override void _Ready()
    {
        _material = (ShaderMaterial)Material;

        UpdateCone();
    }

    public void UpdateCone()
    {
        _material.SetShaderParameter("cone_angle", ConeAngle);
        _material.SetShaderParameter("start_angle", StartAngle);
        _material.SetShaderParameter("fade_intensity", FadeIntensity);
        _material.SetShaderParameter("circle_radius", CircleRadius);
        _material.SetShaderParameter("inner_circle_alpha", InnerCircleAlpha);
        _material.SetShaderParameter("outline_thickness", OutlineThickness);
        _material.SetShaderParameter("cone_scale", ConeScale);
        _material.SetShaderParameter("custom_color", CustomColor);

        Position = - Size * Scale / 2;
    }

    public void OnPressTime(float percentage)
    {
        _material.SetShaderParameter("fade_intensity", FadeIntensity * (1 + percentage));
        Scale = new Vector2(1f + percentage,1f + percentage);
        Position = - Size * Scale / 2;
    }
}
