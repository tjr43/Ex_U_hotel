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

        // 항상 새로운 게임 상태로 초기화
        InitializeNewGame();

        if (gameState != null)
        {
            gameState.currentPlayerId = playerName;
            Debug.Log($"gameState.currentPlayerId가 {gameState.currentPlayerId}(으)로 설정됨");
        }

        SceneManager.LoadScene("GameScene");
    }

    // [수정됨] 초기화 함수: 빈 리스트가 아니라 실제 퀴즈 데이터를 채워넣습니다.
    private void InitializeNewGame()
    {
        Debug.Log("InitializeNewGame() 호출됨. 새 게임 상태를 생성합니다.");

        // 1. GameDataInitializer를 통해 30개 층 데이터를 먼저 생성
        GameState initialState = GameDataInitializer.createInitialState();

        // 2. 생성된 데이터를 기반으로 GameState 설정
        gameState = new GameState(
            1,                  // currentFloor (1층 시작)
            "Player",           // currentPlayerId
            initialState.gameFloors, // [중요] 30개 층 데이터 연결!
            10                  // attemptsLeft
        );

        Debug.Log($"새 GameState 생성 완료. 총 층수: {gameState.gameFloors.Count}");
    }

    // --- 4. GameScene에서 사용될 함수들 ---

    public void LoadGameOrCreateNew()
    {
        if (File.Exists(saveFilePath))
        {
            LoadGameState();
        }
        // 파일이 없거나 로드 실패 시, 이미 StartGame에서 초기화된 gameState를 사용
        if (gameState == null || gameState.gameFloors == null || gameState.gameFloors.Count == 0)
        {
            Debug.LogWarning("저장된 파일이 없거나 데이터가 비어있어 새로 초기화합니다.");
            InitializeNewGame();
        }
    }

    public void LoadGameState()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            gameState = JsonUtility.FromJson<GameState>(json);

            // 로드했는데 층 데이터가 비어있으면 다시 채움 (안전장치)
            if (gameState.gameFloors == null || gameState.gameFloors.Count == 0)
            {
                GameState initialState = GameDataInitializer.createInitialState();
                gameState.gameFloors = initialState.gameFloors;
            }
        }
    }

    public void SubmitAnswer(string answer)
    {
        if (gameState == null) return;
        Debug.Log("답변 제출: " + answer);

        int currentFloorIdx = gameState.currentFloor - 1;
        if (currentFloorIdx < 0 || currentFloorIdx >= gameState.gameFloors.Count) return;

        // 정답 체크 로직
        Floor floorData = gameState.gameFloors[currentFloorIdx];
        if (floorData.traps != null && floorData.traps.Count > 0)
        {
            Trap trap = floorData.traps[0];
            if (answer.Trim() == trap.answer)
            {
                Debug.Log("정답입니다!");

                // 클리어 목록에 추가
                if (!gameState.clearedFloors.Contains(gameState.currentFloor))
                {
                    gameState.clearedFloors.Add(gameState.currentFloor);
                }

                // UI 갱신 요청
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateUI();
                    UIManager.Instance.CloseAllPanels(); // 정답 맞히면 창 닫기
                    UIManager.Instance.ShowInteractionMessage("정답입니다! 다음 층으로 이동하세요.");
                }
                SaveGame();
            }
            else
            {
                Debug.Log("오답입니다.");
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowInteractionMessage("오답입니다.");
                }
                gameState.attemptsLeft--; // 기회 차감 등
            }
        }
    }

    public void ChangeFloor(int floorNumber)
    {
        if (gameState == null) return;

        int totalFloors = gameState.gameFloors.Count;

        // 1. 없는 층 체크
        if (floorNumber < 1 || floorNumber > totalFloors)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowInteractionMessage("존재하지 않는 층입니다.");
            }
            return;
        }

        // 2. 정상 이동
        gameState.currentFloor = floorNumber;
        Debug.Log($"{floorNumber}층으로 이동했습니다.");

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

    [System.Serializable]
    private class MemosWrapper
    {
        public List<PlayerRecord> memos = new List<PlayerRecord>();
    }
}