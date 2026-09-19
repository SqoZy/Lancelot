using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform weapon;
    [SerializeField] private float attackDelay = 0.4f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private LayerMask enemyLayer;
    private bool isAttacking = false;
    private bool readyToAttack = true;
    private int attackCount = 0;

    private void Update()
    {
        
    }
}
