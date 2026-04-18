public interface IAttackPerformer
{
    bool TryPerformAttack(RatController attacker, RatController target);
}