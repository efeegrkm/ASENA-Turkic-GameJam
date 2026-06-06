using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName; // Düşman adı (örneğin: "Kurt", "Ayı")
    public GameObject enemyPrefab; // Düşmanın prefab'ı
    public int maxHealth; // Düşmanın maksimum sağlığı
    public int health; // Düşmanın sağlığı
    public float speed; // Düşmanın hareket hızı
    public int damage; // Düşmanın vereceği hasar
}
