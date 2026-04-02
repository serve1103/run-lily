using System.Collections.Generic;
using UnityEngine;

public class ChunkSpawner : MonoBehaviour
{
    [Header("Chunk Settings")]
    public ChunkData[] easyChunks;
    public ChunkData[] normalChunks;
    public ChunkData[] hardChunks;

    [Header("Spawn Settings")]
    public float spawnAheadDistance = 20f; // 화면 오른쪽 얼마나 앞에 생성할지
    public float minChunkGap = 3f;        // 청크 간 최소 간격

    float nextSpawnX;
    List<ChunkData> recentChunks = new List<ChunkData>(); // 최근 2개 추적

    void Start()
    {
        nextSpawnX = spawnAheadDistance;
    }

    void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        // 다음 청크 생성 위치에 도달하면 스폰
        float cameraRightEdge = Camera.main.transform.position.x + spawnAheadDistance;
        if (nextSpawnX < cameraRightEdge)
        {
            SpawnChunk();
        }
    }

    void SpawnChunk()
    {
        ChunkData chunk = SelectChunk();
        if (chunk == null) return;

        // 청크 내 오브젝트들 스폰
        foreach (ChunkData.SpawnEntry entry in chunk.entries)
        {
            Vector3 pos = new Vector3(
                nextSpawnX + entry.offset.x,
                entry.offset.y,
                0f
            );
            ObjectPool.Instance.Get(entry.poolTag, pos);
        }

        // 최근 청크 기록 (연속 등장 방지)
        recentChunks.Add(chunk);
        if (recentChunks.Count > 2)
            recentChunks.RemoveAt(0);

        // 다음 스폰 위치
        nextSpawnX += minChunkGap + Random.Range(0f, 2f);
    }

    ChunkData SelectChunk()
    {
        // 진행도에 따른 난이도 비율
        float progress = GameManager.Instance.Distance;
        float maxDistance = 500f; // 스테이지 1 기준
        float ratio = Mathf.Clamp01(progress / maxDistance);

        ChunkData[] pool = PickDifficultyPool(ratio);
        if (pool == null || pool.Length == 0) return null;

        // 최근 청크와 겹치지 않게 선택 (최대 10회 시도)
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
        // 기획서 기준: 0~30% → 쉬움70/보통30, 30~70% → 보통50/어려움20, 70~100% → 어려움50
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
