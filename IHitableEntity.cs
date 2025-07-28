using Godot;

public interface IHitableEntity
{
	void OnHit(Node2D sender,float damage);//受攻击时传递伤害以及攻击节点
}