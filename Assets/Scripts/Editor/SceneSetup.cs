using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

public class SceneSetup : Editor
{
    [MenuItem("달려라 릴리/1. 전체 씬 자동 세팅")]
    static void SetupFullScene()
    {
        SetupLayers();
        CreateGround();
        CreatePlayer();
        CreateManagers();
        CreatePrefabs();
        SetupObjectPool();
        CreateChunkData();
        SetupChunkSpawner();
        SetupCamera();
        CreateUI();

        Debug.Log("=== 달려라 릴리 씬 세팅 완료! Play 버튼을 눌러보세요 ===");
    }

    static void SetupLayers()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        // Layer 6 = Ground
        SerializedProperty layer6 = layers.GetArrayElementAtIndex(6);
        if (string.IsNullOrEmpty(layer6.stringValue))
        {
            layer6.stringValue = "Ground";
            tagManager.ApplyModifiedProperties();
            Debug.Log("Layer 6 = Ground 설정 완료");
        }
    }

    static void CreateGround()
    {
        if (GameObject.Find("Ground")) return;

        GameObject ground = new GameObject("Ground");
        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(0.45f, 0.75f, 0.35f); // 초록색 잔디
        ground.transform.position = new Vector3(0f, -4f, 0f);
        ground.transform.localScale = new Vector3(50f, 1f, 1f);
        ground.layer = 6; // Ground layer

        BoxCollider2D col = ground.AddComponent<BoxCollider2D>();

        // 두 번째 바닥 타일 (무한 스크롤용)
        GameObject ground2 = new GameObject("Ground2");
        SpriteRenderer sr2 = ground2.AddComponent<SpriteRenderer>();
        sr2.sprite = CreateSquareSprite();
        sr2.color = new Color(0.45f, 0.75f, 0.35f);
        ground2.transform.position = new Vector3(50f, -4f, 0f);
        ground2.transform.localScale = new Vector3(50f, 1f, 1f);
        ground2.layer = 6;
        ground2.AddComponent<BoxCollider2D>();

        Debug.Log("바닥 생성 완료");
    }

    static void CreatePlayer()
    {
        if (GameObject.Find("Player")) return;

        GameObject player = new GameObject("Player");
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = new Color(1f, 0.85f, 0.65f); // 크림색 (릴리)
        sr.sortingOrder = 10;
        player.transform.position = new Vector3(-3f, -2.5f, 0f);
        player.transform.localScale = new Vector3(1f, 1.5f, 1f);

        // Rigidbody2D
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // 물리 충돌용 콜라이더
        BoxCollider2D physicsCol = player.AddComponent<BoxCollider2D>();

        // 트리거 콜라이더 (간식/장애물 감지)
        BoxCollider2D triggerCol = player.AddComponent<BoxCollider2D>();
        triggerCol.isTrigger = true;
        triggerCol.size = new Vector2(1.2f, 1.2f); // 약간 크게

        // PlayerController
        PlayerController pc = player.AddComponent<PlayerController>();

        // GroundCheck 자식 오브젝트
        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(player.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.8f, 0f);

        // PlayerController 설정
        pc.groundCheck = groundCheck.transform;
        pc.groundLayer = 1 << 6; // Ground layer
        pc.normalCollider = physicsCol;

        Debug.Log("플레이어(릴리) 생성 완료");
    }

    static void CreateManagers()
    {
        // GameManager
        if (!GameObject.Find("GameManager"))
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
            Debug.Log("GameManager 생성 완료");
        }

        // ObjectPool
        if (!GameObject.Find("ObjectPool"))
        {
            GameObject op = new GameObject("ObjectPool");
            op.AddComponent<ObjectPool>();
            Debug.Log("ObjectPool 생성 완료");
        }

        // ChunkSpawner
        if (!GameObject.Find("ChunkSpawner"))
        {
            GameObject cs = new GameObject("ChunkSpawner");
            cs.AddComponent<ChunkSpawner>();
            Debug.Log("ChunkSpawner 생성 완료");
        }
    }

    static void CreatePrefabs()
    {
        string prefabPath = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabPath))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // 울타리 (Fence)
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Fence.prefab"))
        {
            GameObject fence = new GameObject("Fence");
            SpriteRenderer sr = fence.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0.55f, 0.35f, 0.15f); // 갈색
            sr.sortingOrder = 5;
            fence.transform.localScale = new Vector3(0.5f, 1.5f, 1f);

            BoxCollider2D col = fence.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            ObstacleBase obs = fence.AddComponent<ObstacleBase>();
            obs.pushBackDistance = 2f;
            obs.recoveryTime = 1f;
            obs.avoidMethod = ObstacleBase.AvoidMethod.Jump;

            PrefabUtility.SaveAsPrefabAsset(fence, prefabPath + "/Fence.prefab");
            DestroyImmediate(fence);
            Debug.Log("울타리 프리팹 생성 완료");
        }

        // 간판 (Sign)
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Sign.prefab"))
        {
            GameObject sign = new GameObject("Sign");
            SpriteRenderer sr = sign.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0.8f, 0.5f, 0.2f); // 주황색
            sr.sortingOrder = 5;
            sign.transform.localScale = new Vector3(1.5f, 0.5f, 1f);

            BoxCollider2D col = sign.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            ObstacleBase obs = sign.AddComponent<ObstacleBase>();
            obs.pushBackDistance = 2f;
            obs.recoveryTime = 1f;
            obs.avoidMethod = ObstacleBase.AvoidMethod.Slide;

            PrefabUtility.SaveAsPrefabAsset(sign, prefabPath + "/Sign.prefab");
            DestroyImmediate(sign);
            Debug.Log("간판 프리팹 생성 완료");
        }

        // 대형견 (BigDog)
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/BigDog.prefab"))
        {
            GameObject bigDog = new GameObject("BigDog");
            SpriteRenderer sr = bigDog.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0.6f, 0.4f, 0.2f); // 진갈색
            sr.sortingOrder = 5;
            bigDog.transform.localScale = new Vector3(1.5f, 2f, 1f);

            BoxCollider2D col = bigDog.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            ObstacleBase obs = bigDog.AddComponent<ObstacleBase>();
            obs.pushBackDistance = 3f;
            obs.recoveryTime = 1.5f;
            obs.avoidMethod = ObstacleBase.AvoidMethod.Any;

            PrefabUtility.SaveAsPrefabAsset(bigDog, prefabPath + "/BigDog.prefab");
            DestroyImmediate(bigDog);
            Debug.Log("대형견 프리팹 생성 완료");
        }

        // 간식 (Snack)
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Snack.prefab"))
        {
            GameObject snack = new GameObject("Snack");
            SpriteRenderer sr = snack.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(1f, 0.9f, 0.2f); // 노란색
            sr.sortingOrder = 5;
            snack.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

            CircleCollider2D col = snack.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            Snack s = snack.AddComponent<Snack>();
            s.scoreValue = 10;

            PrefabUtility.SaveAsPrefabAsset(snack, prefabPath + "/Snack.prefab");
            DestroyImmediate(snack);
            Debug.Log("간식 프리팹 생성 완료");
        }
    }

    static void SetupObjectPool()
    {
        ObjectPool pool = FindObjectOfType<ObjectPool>();
        if (pool == null || pool.pools != null && pool.pools.Count > 0) return;

        pool.pools = new System.Collections.Generic.List<ObjectPool.Pool>();

        string[] tags = { "fence", "sign", "bigdog", "snack" };
        string[] prefabNames = { "Fence", "Sign", "BigDog", "Snack" };
        int[] sizes = { 5, 5, 3, 20 };

        for (int i = 0; i < tags.Length; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + prefabNames[i] + ".prefab");
            if (prefab != null)
            {
                pool.pools.Add(new ObjectPool.Pool
                {
                    tag = tags[i],
                    prefab = prefab,
                    initialSize = sizes[i]
                });
            }
        }

        EditorUtility.SetDirty(pool);
        Debug.Log("ObjectPool 설정 완료");
    }

    static void CreateChunkData()
    {
        string path = "Assets/ScriptableObjects";
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");

        // 청크 1A: 간식 5개 직선
        CreateChunk(path, "Chunk_1A", "1-A", ChunkData.Difficulty.Easy, new ChunkData.SpawnEntry[]
        {
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(0f, -2.5f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(1.5f, -2.5f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(3f, -2.5f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(4.5f, -2.5f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(6f, -2.5f) },
        });

        // 청크 1B: 울타리 + 간식 3개 (위)
        CreateChunk(path, "Chunk_1B", "1-B", ChunkData.Difficulty.Easy, new ChunkData.SpawnEntry[]
        {
            new ChunkData.SpawnEntry { poolTag = "fence", offset = new Vector2(0f, -3.25f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(0f, -1f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(1.5f, -1f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(3f, -2.5f) },
        });

        // 청크 1C: 간판 + 간식 3개 (아래)
        CreateChunk(path, "Chunk_1C", "1-C", ChunkData.Difficulty.Easy, new ChunkData.SpawnEntry[]
        {
            new ChunkData.SpawnEntry { poolTag = "sign", offset = new Vector2(0f, -1.5f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(0f, -3f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(1.5f, -2.5f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(3f, -2.5f) },
        });

        // 청크 1D: 울타리 → 간판 연속
        CreateChunk(path, "Chunk_1D", "1-D", ChunkData.Difficulty.Normal, new ChunkData.SpawnEntry[]
        {
            new ChunkData.SpawnEntry { poolTag = "fence", offset = new Vector2(0f, -3.25f) },
            new ChunkData.SpawnEntry { poolTag = "sign", offset = new Vector2(3f, -1.5f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(1.5f, -1f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(4.5f, -3f) },
        });

        // 청크 1E: 대형견 + 소고기
        CreateChunk(path, "Chunk_1E", "1-E", ChunkData.Difficulty.Normal, new ChunkData.SpawnEntry[]
        {
            new ChunkData.SpawnEntry { poolTag = "bigdog", offset = new Vector2(0f, -3f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(3f, -2.5f) },
            new ChunkData.SpawnEntry { poolTag = "snack", offset = new Vector2(4.5f, -2.5f) },
        });

        Debug.Log("청크 데이터 5개 생성 완료");
    }

    static void CreateChunk(string path, string fileName, string id, ChunkData.Difficulty diff, ChunkData.SpawnEntry[] entries)
    {
        string fullPath = path + "/" + fileName + ".asset";
        if (AssetDatabase.LoadAssetAtPath<ChunkData>(fullPath)) return;

        ChunkData chunk = ScriptableObject.CreateInstance<ChunkData>();
        chunk.chunkId = id;
        chunk.difficulty = diff;
        chunk.entries = entries;

        AssetDatabase.CreateAsset(chunk, fullPath);
    }

    static void SetupChunkSpawner()
    {
        ChunkSpawner spawner = FindObjectOfType<ChunkSpawner>();
        if (spawner == null) return;

        // ScriptableObjects에서 청크 로드
        ChunkData chunk1A = AssetDatabase.LoadAssetAtPath<ChunkData>("Assets/ScriptableObjects/Chunk_1A.asset");
        ChunkData chunk1B = AssetDatabase.LoadAssetAtPath<ChunkData>("Assets/ScriptableObjects/Chunk_1B.asset");
        ChunkData chunk1C = AssetDatabase.LoadAssetAtPath<ChunkData>("Assets/ScriptableObjects/Chunk_1C.asset");
        ChunkData chunk1D = AssetDatabase.LoadAssetAtPath<ChunkData>("Assets/ScriptableObjects/Chunk_1D.asset");
        ChunkData chunk1E = AssetDatabase.LoadAssetAtPath<ChunkData>("Assets/ScriptableObjects/Chunk_1E.asset");

        spawner.easyChunks = new ChunkData[] { chunk1A, chunk1B, chunk1C };
        spawner.normalChunks = new ChunkData[] { chunk1D, chunk1E };
        spawner.hardChunks = new ChunkData[] { chunk1D, chunk1E }; // 임시로 같은 것

        EditorUtility.SetDirty(spawner);
        Debug.Log("ChunkSpawner 설정 완료");
    }

    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f); // 하늘색
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(0f, 0f, -10f);

        Debug.Log("카메라 설정 완료");
    }

    static void CreateUI()
    {
        if (GameObject.Find("GameUI")) return;

        // Canvas
        GameObject canvasObj = new GameObject("GameUI");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Score Text
        GameObject scoreObj = CreateUIText(canvasObj.transform, "ScoreText", "0",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(0f, -10f), new Vector2(200f, 50f));

        // Distance Text
        GameObject distObj = CreateUIText(canvasObj.transform, "DistanceText", "0m",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-10f, -10f), new Vector2(200f, 50f));

        // Snack Text
        GameObject snackObj = CreateUIText(canvasObj.transform, "SnackText", "0/50",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-10f, -50f), new Vector2(200f, 50f));

        // Game Over Panel
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform goRT = gameOverPanel.AddComponent<RectTransform>();
        goRT.anchorMin = Vector2.zero;
        goRT.anchorMax = Vector2.one;
        goRT.sizeDelta = Vector2.zero;

        Image goBg = gameOverPanel.AddComponent<Image>();
        goBg.color = new Color(0f, 0f, 0f, 0.7f);

        // Game Over Title
        CreateUIText(gameOverPanel.transform, "GameOverTitle", "게임 오버!",
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(300f, 60f), 36);

        // Final Score
        GameObject finalScore = CreateUIText(gameOverPanel.transform, "FinalScoreText", "점수: 0",
            new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(300f, 40f), 24);

        // Final Distance
        GameObject finalDist = CreateUIText(gameOverPanel.transform, "FinalDistanceText", "거리: 0m",
            new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(300f, 40f), 24);

        // Final Snack
        GameObject finalSnack = CreateUIText(gameOverPanel.transform, "FinalSnackText", "간식: 0개",
            new Vector2(0.5f, 0.41f), new Vector2(0.5f, 0.41f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(300f, 40f), 24);

        // Retry Button
        GameObject retryBtn = CreateUIButton(gameOverPanel.transform, "RetryButton", "다시 하기",
            new Vector2(0.5f, 0.25f), new Vector2(200f, 50f));

        gameOverPanel.SetActive(false);

        // GameUI 컴포넌트
        GameUI gameUI = canvasObj.AddComponent<GameUI>();
        gameUI.scoreText = scoreObj.GetComponent<Text>();
        gameUI.distanceText = distObj.GetComponent<Text>();
        gameUI.snackText = snackObj.GetComponent<Text>();
        gameUI.gameOverPanel = gameOverPanel;
        gameUI.finalScoreText = finalScore.GetComponent<Text>();
        gameUI.finalDistanceText = finalDist.GetComponent<Text>();
        gameUI.finalSnackText = finalSnack.GetComponent<Text>();
        gameUI.retryButton = retryBtn.GetComponent<Button>();

        Debug.Log("UI 생성 완료");
    }

    static GameObject CreateUIText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 position, Vector2 size, int fontSize = 20)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text t = obj.AddComponent<Text>();
        t.text = text;
        t.fontSize = fontSize;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return obj;
    }

    static GameObject CreateUIButton(Transform parent, string name, string label,
        Vector2 anchor, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f);

        Button btn = btnObj.AddComponent<Button>();

        // Button label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.sizeDelta = Vector2.zero;

        Text t = labelObj.AddComponent<Text>();
        t.text = label;
        t.fontSize = 20;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return btnObj;
    }

    // 기본 사각형 스프라이트 생성
    static Sprite CreateSquareSprite()
    {
        string path = "Assets/Sprites/Square.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing) return existing;

        if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
            AssetDatabase.CreateFolder("Assets", "Sprites");

        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 4;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // 기본 원형 스프라이트 생성
    static Sprite CreateCircleSprite()
    {
        string path = "Assets/Sprites/Circle.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing) return existing;

        if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
            AssetDatabase.CreateFolder("Assets", "Sprites");

        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f - 1;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                pixels[y * size + x] = dist <= radius ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
