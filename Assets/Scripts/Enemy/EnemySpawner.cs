using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    // Singleton instance for easy access from other scripts
    public static EnemySpawner Instance { get; private set; }

    [Header("Enemy Types")]
    [SerializeField] private List<EnemyData> enemyTypes;

    [Header("Difficulty Settings (Zone 1, 2, 3)")]
    [SerializeField] private float[] spawnIntervalsPerZone = {7f, 7f, 7f};
    [SerializeField] private int[] enemiesPerWavePerZone = {3, 4, 5}; 
    [SerializeField] private float[] restDurationPerZone = {15f, 15f, 13f};

    [Header("Spawn Points")]
    [SerializeField] private Transform[] zone1SpawnPoints;
    [SerializeField] private Transform[] zone2SpawnPoints;
    [SerializeField] private Transform[] zone3SpawnPoints;

    [Header("Zones")]
    [SerializeField] private MeshCollider zone1;
    [SerializeField] private MeshCollider zone2;
    [SerializeField] private MeshCollider zone3;

    private MeshCollider currentZoneCollider;

    //State machine to handle spawning and resting periods
    private enum SpawnerState {Spawning, Resting, Tutorial}
    private SpawnerState currentState = SpawnerState.Spawning;

    private int currentZone = 1;
    private float spawnTimer;
    private float restTimer;
    private int enemiesSpawnedInCurrentWave = 0;

    public float tutorialDuration = 4f; // Panelin ekranda kalacağı saniye
    private float tutorialTimer;

    private bool isFirstEnemySent = false;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SendFirstEnemy();
    }

    private void Update()
    {
        if (!isFirstEnemySent) return;

        // Yeni durum makinesi kontrolü
        if (currentState == SpawnerState.Spawning)
        {
            HandleSpawning();
        }
        else if (currentState == SpawnerState.Resting)
        {
            HandleResting();
        }
        else if (currentState == SpawnerState.Tutorial)
        {
            HandleTutorial();
        }
    }

    private void HandleSpawning()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnEnemy();
            enemiesSpawnedInCurrentWave++;

            // Check if the current wave has reached its target enemy count
            int targetEnemyCount = enemiesPerWavePerZone[currentZone - 1];
            if (enemiesSpawnedInCurrentWave >= targetEnemyCount)
            {
                // Switch to resting state to give player time for the extra task
                currentState = SpawnerState.Resting;
                restTimer = restDurationPerZone[currentZone - 1];
                Debug.Log($"Wave cleared! Rest phase started for {restTimer} seconds.");
            }
            else
            {
                // Reset spawn timer for the next enemy in the current wave
                spawnTimer = spawnIntervalsPerZone[currentZone - 1];
            }
        }
    }

    private void HandleResting()
    {
        restTimer -= Time.deltaTime;
        if (restTimer <= 0)
        {
            // Rest time is over, start the next wave of enemies
            StartNewWave();
        }
    }

    private void SendFirstEnemy()
    {
        SpawnEnemy();
        enemiesSpawnedInCurrentWave++; // İlk düşmanı da mevcut dalga sayısına dahil ediyoruz
        isFirstEnemySent = true;

        // Normal doğmayı değil, önce Tutorial aşamasını başlatıyoruz
        currentState = SpawnerState.Tutorial;
        tutorialTimer = tutorialDuration;

    }

    private void HandleTutorial()
    {
        tutorialTimer -= Time.deltaTime;
        if (tutorialTimer <= 0)
        {
            GameEvents.OnShowHint("Düşmanlar bebeğe saldırmadan onu koru!", tutorialDuration);
        }
        // Bilgilendirme bitti, artık normal doğma döngüsüne geçebiliriz
        currentState = SpawnerState.Spawning;
            
        // Bir sonraki düşmanın ne zaman geleceğini belirle
        spawnTimer = spawnIntervalsPerZone[currentZone - 1]; 
    }

    private void StartNewWave()
    {
        GameEvents.OnPlayMusic("FightMusic");
        currentState = SpawnerState.Spawning;
        enemiesSpawnedInCurrentWave = 0;
        spawnTimer = spawnIntervalsPerZone[currentZone - 1];
        Debug.Log($"New Wave Started! Active Zone: {currentZone}");
    }

    // Triggered by invisible DifficultyZone planes when the player crosses them
    public void SetDifficultyLevel(int newZone)
    {
        if (newZone == currentZone) return; 

        currentZone = newZone;
        
        // Immediately start a fresh wave tailored to the new zone
        StartNewWave();
    }

    private void SpawnEnemy()
    {
        //Get a random spawn point ONLY from the active zone
        currentZoneCollider = GetActiveZoneCollider();
        Transform randomPoint = GetRandomPointForCurrentZone();
        if (randomPoint == null) return;

        EnemyData selectedEnemy = ChooseEnemyByDifficulty();

        // Instantiate and initialize the enemy (Object Pooling is recommended here for final polish)
        GameObject enemyObj = Instantiate(selectedEnemy.enemyPrefab, randomPoint.position, randomPoint.rotation);
        Enemy enemyScript = enemyObj.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.Initialize(selectedEnemy);
        }
    }

    private MeshCollider GetActiveZoneCollider()
    {
        if (IsInsideZone(transform.position, zone1)) return zone1;
        else if (IsInsideZone(transform.position, zone2)) return zone2;
        else if (IsInsideZone(transform.position, zone3)) return zone3;

        Debug.LogWarning("Player is not inside any defined zone! Defaulting to Zone 1 collider.");
        return zone1; // Default to Zone 1 if somehow outside all zones
    }

    private bool IsInsideZone(Vector3 point, MeshCollider planeRenderer)
    {
        Bounds bounds = planeRenderer.bounds;

        bool isInsideXBounds = point.x >= bounds.min.x && point.x <= bounds.max.x;
        bool isInsideZBounds = point.z >= bounds.min.z && point.z <= bounds.max.z;

        return isInsideXBounds && isInsideZBounds;
    }

    private Transform GetRandomPointForCurrentZone()
    {
        Transform[] activeList = currentZoneCollider == zone1 ? zone1SpawnPoints : (currentZoneCollider == zone2 ? zone2SpawnPoints : zone3SpawnPoints);
        
        if (activeList.Length == 0)
        {
            Debug.LogError($"No spawn points found for Zone {currentZone}!");
            return null;
        }
        int randomIndex = Random.Range(0, activeList.Length);
        Debug.Log($"Selected spawn point {activeList[randomIndex].name} for Zone {currentZone}");
        return activeList[randomIndex];
    }

    private EnemyData ChooseEnemyByDifficulty()
    {
        int randomIndex = 0;
        // Logic to dynamically pick enemy types based on the current zone progression
        if (currentZone == 1) randomIndex = 0;
        //else if (currentZone == 2) randomIndex = Random.Range(0, Mathf.Min(2, enemyTypes.Count));
        //else randomIndex = Random.Range(0, enemyTypes.Count);

        return enemyTypes[randomIndex];
    }
}