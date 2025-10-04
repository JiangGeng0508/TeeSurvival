using Godot;

public interface IHitableEntity
{
	void OnHit(Vector2 direction,float damage);//受攻击时传递伤害以及攻击方向
}