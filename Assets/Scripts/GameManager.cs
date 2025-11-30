using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // --- 1. 싱글톤 및 기본 변수 ---
    public static GameManager Instance { get; private set; }
    public GameState gameState; // DataModels.cs에서 정의된 것을 사용

    private string saveFilePath;
    private string memoFilePath;

    // --- 2. 초기화 (Awake) ---
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // StartScene부터 GameScene까지 유지

        saveFilePath = Path.Combine(Application.persistentDataPath, "gameState.json");
        memoFilePath = Path.Combine(Application.persistentDataPath, "memos.json");
    }

    // --- 3. StartScene에서 호출될 함수 ---
    public void StartGame(string playerName)
    {
        Debug.Log($"StartGame 호출됨. 플레이어 이름: {playerName}");

        InitializeNewGame();

        if (gameState != null)
        {
            gameState.currentPlayerId = playerName;
        }

        SceneManager.LoadScene("GameScene");
    }

    // 초기화 함수
    private void InitializeNewGame()
    {
        Debug.Log("InitializeNewGame() 호출됨.");

        // 30개 층 데이터 생성
        GameState initialState = GameDataInitializer.createInitialState();

        // ▼▼▼ [수정 1] 목숨을 2개로 설정 ▼▼▼
        gameState = new GameState(
            1,
            "Player",
            initialState.gameFloors,
            2                   // attemptsLeft: 2 (기회 2번)
        );
        // ▲▲▲

        Debug.Log("새 게임 상태 생성 완료 (목숨 2개)");
    }

    // --- 4. GameScene에서 사용될 함수들 ---

    public void LoadGameOrCreateNew()
    {
        if (File.Exists(saveFilePath))
        {
            LoadGameState();
        }
        if (gameState == null || gameState.gameFloors == null || gameState.gameFloors.Count == 0)
        {
            InitializeNewGame();
        }
    }

    public void LoadGameState()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            gameState = JsonUtility.FromJson<GameState>(json);

            if (gameState.gameFloors == null || gameState.gameFloors.Count == 0)
            {
                GameState initialState = GameDataInitializer.createInitialState();
                gameState.gameFloors = initialState.gameFloors;
            }
        }
    }

    // ▼▼▼ [수정 2] 정답/오답 및 게임오버 처리 로직 ▼▼▼
    public void SubmitAnswer(string answer)
    {
        if (gameState == null) return;
        Debug.Log("답변 제출: " + answer);

        int currentFloorIdx = gameState.currentFloor - 1;
        if (currentFloorIdx < 0 || currentFloorIdx >= gameState.gameFloors.Count) return;

        Floor floorData = gameState.gameFloors[currentFloorIdx];

        if (floorData.traps != null && floorData.traps.Count > 0)
        {
            Trap trap = floorData.traps[0];

            // [정답 처리]
            if (answer.Trim() == trap.answer)
            {
                Debug.Log("정답입니다! 승리!");

                // 정답이면 바로 WinScene으로 이동
                SceneManager.LoadScene("WinScene");
            }
            // [오답 처리]
            else
            {
                gameState.attemptsLeft--; // 목숨 1개 차감
                Debug.Log($"오답입니다. 남은 목숨: {gameState.attemptsLeft}");

                // 목숨이 다 떨어졌는지 확인
                if (gameState.attemptsLeft <= 0)
                {
                    Debug.Log("모든 기회 소진. 게임 오버!");
                    SceneManager.LoadScene("GameOverScene");
                }
                else
                {
                    // 기회가 남았다면 UI 갱신해서 알려줌
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.UpdateUI(); // 남은 목숨 갱신
                        UIManager.Instance.ShowInteractionMessage($"틀렸습니다! 남은 기회: {gameState.attemptsLeft}번");
                    }
                }
            }
            SaveGame(); // 상태 저장 (목숨 깎인 것 등)
        }
    }
    // ▲▲▲ [수정 완료] ▲▲▲

    public void ChangeFloor(int floorNumber)
    {
        if (gameState == null) return;

        int totalFloors = gameState.gameFloors.Count;

        if (floorNumber < 1 || floorNumber > totalFloors)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowInteractionMessage("존재하지 않는 층입니다.");
            return;
        }

        gameState.currentFloor = floorNumber;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowInteractionMessage($"{floorNumber}층에 도착했습니다.");
        }
        SaveGame();
    }

    public void SaveGame()
    {
        if (gameState == null) return;
        string json = JsonUtility.ToJson(gameState, true);
        File.WriteAllText(saveFilePath, json);
    }

    public List<PlayerRecord> LoadMemos()
    {
        if (File.Exists(memoFilePath))
        {
            string json = File.ReadAllText(memoFilePath);
            MemosWrapper wrapper = JsonUtility.FromJson<MemosWrapper>(json) ?? new MemosWrapper();
            return wrapper.memos;
        }
        return new List<PlayerRecord>();
    }

    public void SaveMemoAndExit(string memo, string status)
    {
        if (gameState == null) return;

        PlayerRecord record = new PlayerRecord
        {
            playerId = gameState.currentPlayerId,
            status = status,
            memo = memo,
            timestamp = System.DateTime.Now.ToString()
        };

        if (gameState.playerHistory == null)
        {
            gameState.playerHistory = new List<PlayerRecord>();
        }
        gameState.playerHistory.Add(record);

        List<PlayerRecord> allMemos = LoadMemos();
        allMemos.Add(record);
        MemosWrapper wrapper = new MemosWrapper { memos = allMemos };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(memoFilePath, json);

        SaveGame();
        SceneManager.LoadScene("GoodbyeScene");
    }

    public void ResetGameData()
    {
        if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
        if (File.Exists(memoFilePath)) File.Delete(memoFilePath);

        InitializeNewGame();
        SceneManager.LoadScene("GameScene");
        Debug.Log("게임 리셋 완료!");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ResetGameData();
        }
    }

    [System.Serializable]
    private class MemosWrapper
    {
        public List<PlayerRecord> memos = new List<PlayerRecord>();
    }
}