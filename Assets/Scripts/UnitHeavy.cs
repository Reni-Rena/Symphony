using UnityEngine;

public enum AttackTypeHeavy
{
    Basic
}

public class UnitHeavy : MonoBehaviour
{
    public AttackTypeHeavy attackType = AttackTypeHeavy.Basic;
    public int protectedCase = 1;
}
