using UnityEngine;

[CreateAssetMenu(fileName = "NewChunk", menuName = "RunLily/Chunk Data")]
public class ChunkData : ScriptableObject
{
    public string chunkId;
    public Difficulty difficulty = Difficulty.Easy;

    [System.Serializable]
    public class SpawnEntry
    {
        public string poolTag; // ObjectPool에서 가져올 태그
        public Vector2 offset; // 청크 시작점 기준 상대 위치
    }

    public SpawnEntry[] entries;

    public enum Difficulty { Easy, Normal, Hard }
}
