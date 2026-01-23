public enum MonsterType
{
    Melee,
    Ranged
}

[System.Serializable]
public class MonsterStats
{
    public MonsterType type;
    public int level;

    public int maxHP;
    public int currentHP;

    public int attackDamage;
    public float attackSpeed;
    public float attackRange;

    public float moveSpeed;
}