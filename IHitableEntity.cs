using Godot;

public interface IHitableEntity
{
	void OnHit(Node2D sender,float damage);
}