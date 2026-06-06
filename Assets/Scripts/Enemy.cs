using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyData data;
    private int currentHealth;

    public void Initialize(EnemyData enemyData)
    {
        data = enemyData;
        currentHealth = data.maxHealth;
    }
}