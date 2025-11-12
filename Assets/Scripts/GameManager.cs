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
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // StartScene부터 GameScene까지 유지

        saveFilePath = Path.Combine(Application.persistentDataPath, "gameState.json");
        memoFilePath = Path.Combine(Application.persistentDataPath, "memos.json");
    }

    // --- 3. StartScene에서 호출될 함수 ---
    public void StartGame(string playerName) {
        Debug.Log($"StartGame 호출됨. 플레이어 이름: {playerName}");
        if (gameState == null) {
            InitializeNewGame();
        }
        if (gameState != null) {
            gameState.currentPlayerId = playerName;
            Debug.Log($"gameState.currentPlayerId가 {gameState.currentPlayerId}(으)로 설정됨");
        } else {
            Debug.LogError("gameState가 null입니다! InitializeNewGame()이 실패했습니다.");
        }
        SceneManager.LoadScene("GameScene");
    }

    // StartGame의 '도우미 함수'
    private void InitializeNewGame() {
        Debug.Log("InitializeNewGame() 호출됨. 새 게임 상태를 생성합니다.");

        // DataModels.cs에 추가한 생성자를 올바르게 호출
        gameState = new GameState(
            1,                  // currentFloor
            "Player",           // currentPlayerId (StartGame에서 덮어쓸 예정)
            new List<Floor>(),  // gameFloors (GameScene에서 로드할 예정)
            10                  // attemptsLeft
        );
        Debug.Log("새 GameState 객체 생성 및 초기화 완료.");
    }

    // --- 4. GameScene에서 사용될 기존 함수들 ---

    public GameState LoadGameOrCreateNew() {
        if (File.Exists(saveFilePath)) {
            return LoadGameState();
        } else {
            if (gameState == null) {
                Debug.LogError("GameScene이 시작되었지만 gameState가 null입니다. StartGame부터 다시 시작합니다.");
                InitializeNewGame(); // 비상시 fallback
            }
            // GameDataInitializer를 사용해 층 정보 로드 (GameScene에만 필요)
            if (gameState.gameFloors == null || gameState.gameFloors.Count <= 1) {
                // GameDataInitializer.cs의 createInitialState()를 호출합니다.
                // 이 함수는 30개의 층 정보를 생성합니다.
                // ※ GameDataInitializer.cs가 static이 아니면 new()가 필요할 수 있습니다.
                // ※ 'createInitialState' 이름이 다르면 해당 이름으로 수정해야 합니다.

                // GameDataInitializer.cs의 함수가 static 'createInitialState'라고 가정합니다.
                GameState initialState = GameDataInitializer.createInitialState();
                gameState.gameFloors = initialState.gameFloors;
                gameState.attemptsLeft = initialState.attemptsLeft; // (선택사항) 시도 횟수도 여기서 가져옴
                Debug.Log("GameDataInitializer로부터 층 정보를 로드했습니다.");
            }
            return gameState;
        }
    }

    public GameState LoadGameState() {
        if (File.Exists(saveFilePath)) {
            string json = File.ReadAllText(saveFilePath);
            gameState = JsonUtility.FromJson<GameState>(json);
            return gameState;
        }
        return null;
    }

    public void SubmitAnswer(string answer) {
        if (gameState == null) return;
        Debug.Log("답변 제출: " + answer);
        // ... (퀴즈 정답 로직) ...
        // 예: if (answer == "정답") { gameState.clearedFloors.Add(gameState.currentFloor); }
    }

    public void ChangeFloor(int floorNumber) {
        if (gameState == null) return;
        Debug.Log("층 이동: " + floorNumber);
        // ... (층 이동 로직) ...
        // 예: gameState.currentFloor = floorNumber;
    }

    public void SaveGame() {
        if (gameState == null) return;
        string json = JsonUtility.ToJson(gameState, true);
        File.WriteAllText(saveFilePath, json);
    }

    public List<PlayerRecord> LoadMemos() {
        // (참고: UIManager는 gameState.playerHistory에서 직접 로드하므로, 이 함수는 현재 사용되지 않을 수 있습니다)
        if (File.Exists(memoFilePath)) {
            string json = File.ReadAllText(memoFilePath);
            MemosWrapper wrapper = JsonUtility.FromJson<MemosWrapper>(json) ?? new MemosWrapper();
            return wrapper.memos;
        }
        return new List<PlayerRecord>();
    }

    // ▼▼▼ [오류 수정] 비어있던 함수 내용 채우기 ▼▼▼
    public void SaveMemoAndExit(string memo, string status) {
        if (gameState == null) return;

        // 1. 현재 플레이어의 기록(메모) 생성
        PlayerRecord record = new PlayerRecord {
            playerId = gameState.currentPlayerId,
            status = status, // "success" or "fail"
            memo = memo,
            timestamp = System.DateTime.Now.ToString()
        };

        // 2. gameState의 playerHistory에 이 기록 추가
        if (gameState.playerHistory == null) {
            gameState.playerHistory = new List<PlayerRecord>();
        }
        gameState.playerHistory.Add(record);

        // 3. (선택사항) 메모를 별도 파일로도 저장 (LoadMemos 함수와 연동 시)
        List<PlayerRecord> allMemos = LoadMemos();
        allMemos.Add(record);
        MemosWrapper wrapper = new MemosWrapper { memos = allMemos };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(memoFilePath, json);

        // 4. 게임 저장 및 씬 이동
        SaveGame(); // 현재 gameState (history 포함)를 저장
        SceneManager.LoadScene("GoodbyeScene");
    }
    // ▲▲▲ [함수 내용 채우기 완료] ▲▲▲

    // LoadMemos를 위한 Helper Wrapper 클래스
    [System.Serializable]
    private class MemosWrapper
    {
        public List<PlayerRecord> memos = new List<PlayerRecord>();
    }
}