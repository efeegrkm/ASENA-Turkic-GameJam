using UnityEngine;
using UnityEngine.AI;

public class YelbegenSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject yelbegenPrefab;
    [Tooltip("Arkamýzda bakmaya baþlayacaðý en uzak mesafe")]
    [SerializeField] private float maxSpawnDistance = 30f;
    [Tooltip("Zemin bulamazsa en fazla ne kadar yakýna gelsin?")]
    [SerializeField] private float minSpawnDistance = 10f;

    [SerializeField] private Transform forestEscapePoint;

    private bool hasSpawned = false;

    private void OnEnable()
    {
        GameEvents.OnBabyCrying += HandleBabyCrying;
    }

    private void OnDisable()
    {
        GameEvents.OnBabyCrying -= HandleBabyCrying;
    }

    private void HandleBabyCrying(bool isCrying)
    {
        if (isCrying && !hasSpawned)
        {
            SpawnYelbegen();
        }
    }

    private void SpawnYelbegen()
    {
        Transform camTransform = Camera.main.transform;
        Vector3 spawnDirection = -camTransform.forward;
        spawnDirection.y = 0;
        spawnDirection.Normalize();

        bool foundSpawn = false;

        // ÇÖZÜM 3 & 4: 30 metreden baþlayýp 5'er metre yaklaþarak Raycast ile Collider (Zemin) ara
        for (float dist = maxSpawnDistance; dist >= minSpawnDistance; dist -= 5f)
        {
            Vector3 testPos = camTransform.position + (spawnDirection * dist);

            // Yukarýdan aþaðýya doðru bir ýþýn fýrlatýyoruz
            if (Physics.Raycast(testPos + Vector3.up * 10f, Vector3.down, out RaycastHit groundHit, 30f))
            {
                // Bulduðumuz Collider'ýn üzerinde NavMesh (Yürünebilir alan) var mý kontrolü
                if (NavMesh.SamplePosition(groundHit.point, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
                {
                    // Güvenli zemin bulundu!
                    GameObject yelbegen = Instantiate(yelbegenPrefab, navHit.position, Quaternion.LookRotation(camTransform.position - navHit.position));

                    if (yelbegen.TryGetComponent<YelbegenAI>(out YelbegenAI ai))
                    {
                        ai.SetForestPoint(forestEscapePoint);
                    }

                    GameEvents.OnPlayOneShotSFX("YelbegenSpawnRoar");
                    GameEvents.OnShowHint("Yelbegen aðlama sesini duydu...", 5f);

                    foundSpawn = true;
                    hasSpawned = true;
                    break; // Zemin bulunduðu için döngüyü bitir
                }
            }
        }

        // Eðer en dibe (minSpawnDistance) kadar geldik ve hala boþluktaysak
        if (!foundSpawn)
        {
            hasSpawned = false; // Bir sonraki frame'de aðlama devam ediyorsa tekrar yer arar
        }
    }
}