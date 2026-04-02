using System.Collections.Generic;
using UnityEngine;

public class ChunkSpawner : MonoBehaviour
{
    [Header("Chunk Settings")]
    public ChunkData[] easyChunks;
    public ChunkData[] normalChunks;
    public ChunkData[] hardChunks;

    [Header("Spawn Settings")]
    public float spawnX = 15f;           // 화면 오른쪽 밖에서 생성
    public float minChunkGap = 3f;       // 청크 간 최소 간격 (거리 기준)

    float distanceSinceLastSpawn;
    float nextSpawnAfter;
    List<ChunkData> recentChunks = new List<ChunkData>();

    void Start()
    {
        // 첫 청크는 바로 생성
        nextSpawnAfter = 0f;
        distanceSinceLastSpawn = 0f;
    }

    void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        distanceSinceLastSpawn += GameManager.Instance.CurrentSpeed * Time.deltaTime;

        if (distanceSinceLastSpawn >= nextSpawnAfter)
        {
            SpawnChunk();
            distanceSinceLastSpawn = 0f;
            nextSpawnAfter = minChunkGap + Random.Range(0f, 2f);
        }
    }

    void SpawnChunk()
    {
        ChunkData chunk = SelectChunk();
        if (chunk == null)
        {
            Debug.LogWarning("청크 데이터가 없습니다! Inspector에서 청크를 할당해주세요.");
            return;
        }

        foreach (ChunkData.SpawnEntry entry in chunk.entries)
        {
            Vector3 pos = new Vector3(
                spawnX + entry.offset.x,
                entry.offset.y,
                0f
            );
            ObjectPool.Instance.Get(entry.poolTag, pos);
        }

        recentChunks.Add(chunk);
        if (recentChunks.Count > 2)
            recentChunks.RemoveAt(0);
    }

    ChunkData SelectChunk()
    {
        float progress = GameManager.Instance.Distance;
        float maxDistance = 500f;
        float ratio = Mathf.Clamp01(progress / maxDistance);

        ChunkData[] pool = PickDifficultyPool(ratio);
        if (pool == null || pool.Length == 0) return null;

        for (int i = 0; i < 10; i++)
        {
            ChunkData candidate = pool[Random.Range(0, pool.Length)];
            if (!recentChunks.Contains(candidate))
                return candidate;
        }

        return pool[Random.Range(0, pool.Length)];
    }

    ChunkData[] PickDifficultyPool(float ratio)
    {
        float roll = Random.value;

        if (ratio < 0.3f)
        {
            return roll < 0.7f ? easyChunks : normalChunks;
        }
        else if (ratio < 0.7f)
        {
            if (roll < 0.3f) return easyChunks;
            if (roll < 0.8f) return normalChunks;
            return hardChunks;
        }
        else
        {
            if (roll < 0.1f) return easyChunks;
            if (roll < 0.5f) return normalChunks;
            return hardChunks;
        }
    }
}
