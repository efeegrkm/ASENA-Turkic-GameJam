using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    // Singleton instance for easy access from other scripts
    public static EnemySpawner Instance { get; private set; }

    [Header("Enemy Types")]
    [SerializeField] private List<EnemyData> enemyTypes;

    [Header("Difficulty Settings (Zone 1, 2, 3)")]
    [SerializeField] private float[] spawnIntervalsPerZone = {4.0f, 2.5f, 1.2f};
    [SerializeField] private int[] enemiesPerWavePerZone = {5, 8, 12}; 
    [SerializeField] private float[] restDurationPerZone = {15f, 10f, 7f};

    [Header("Spawn Points")]
    [SerializeField] private Transform[] zone1SpawnPoints;
    [SerializeField] private Transform[] zone2SpawnPoints;
    [SerializeField] private Transform[] zone3SpawnPoints;

    //State machine to handle spawning and resting periods
    private enum SpawnerState {Spawning, Resting}
    private SpawnerState currentState = SpawnerState.Spawning;

    private int currentZone = 1;
    private float spawnTimer;
    private float restTimer;
    private int enemiesSpawnedInCurrentWave = 0;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartNewWave();
    }

    private void Update()
    {
        if (currentState == SpawnerState.Spawning)
        {
            HandleSpawning();
        }
        else if (currentState == SpawnerState.Resting)
        {
            HandleResting();
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

    private void StartNewWave()
    {
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

    private Transform GetRandomPointForCurrentZone()
    {
        Transform[] activeList = currentZone == 1 ? zone1SpawnPoints : (currentZone == 2 ? zone2SpawnPoints : zone3SpawnPoints);
        
        if (activeList.Length == 0)
        {
            Debug.LogError($"No spawn points found for Zone {currentZone}!");
            return null;
        }
        return activeList[Random.Range(0, activeList.Length)];
    }

    private EnemyData ChooseEnemyByDifficulty()
    {
        int randomIndex = 0;
        // Logic to dynamically pick enemy types based on the current zone progression
        if (currentZone == 1) randomIndex = 0;
        else if (currentZone == 2) randomIndex = Random.Range(0, Mathf.Min(2, enemyTypes.Count));
        else randomIndex = Random.Range(0, enemyTypes.Count);

        return enemyTypes[randomIndex];
    }
}