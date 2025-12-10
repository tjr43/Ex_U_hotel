using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState gameState;

    private string saveFilePath;
    private string memoFilePath;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "gameState.json");
        memoFilePath = Path.Combine(Application.persistentDataPath, "memos.json");
    }

    public void StartGame(string playerName) {
        InitializeNewGame();
        if (gameState != null) gameState.currentPlayerId = playerName;
        SceneManager.LoadScene("GameScene");
    }

    private void InitializeNewGame() {
        GameState initialState = GameDataInitializer.createInitialState();
        gameState = new GameState(1, "Player", initialState.gameFloors, 2);
        gameState.playerHistory = LoadMemos();
    }

    public void LoadGameOrCreateNew() {
        if (File.Exists(saveFilePath)) LoadGameState();
        if (gameState == null || gameState.gameFloors == null || gameState.gameFloors.Count == 0) InitializeNewGame();
    }

    public void LoadGameState() {
        if (File.Exists(saveFilePath)) {
            try {
                string json = File.ReadAllText(saveFilePath);
                gameState = JsonUtility.FromJson<GameState>(json);

                if (gameState.gameFloors == null || gameState.gameFloors.Count < 30) {
                    GameState initialState = GameDataInitializer.createInitialState();
                    gameState.gameFloors = initialState.gameFloors;
                }

                if (gameState.playerHistory == null) gameState.playerHistory = LoadMemos();
            } catch (Exception e) {
                Debug.LogError("로드 오류: " + e.Message);
                InitializeNewGame();
            }
        }
    }

    public void SubmitAnswer(string answer) {
        if (gameState == null) return;

        int currentFloorIdx = gameState.currentFloor - 1;
        if (currentFloorIdx < 0 || currentFloorIdx >= gameState.gameFloors.Count) return;

        Floor floorData = gameState.gameFloors[currentFloorIdx];

        if (floorData.traps != null && floorData.traps.Count > 0) {
            Trap trap = floorData.traps[0];

            if (answer.Trim() == trap.answer) {
                if (UIManager.Instance != null) UIManager.Instance.CloseAllPanels();
                StartCoroutine(ProcessCorrectAnswerRoutine());
            } else {
                // 오답 처리 로직 (공통 함수로 분리 가능하지만 일단 유지)
                ProcessWrongAnswer("틀렸습니다!");
            }
            SaveGame();
        }
    }

    // ▼▼▼ [추가됨] 시간 초과 시 호출될 함수 ▼▼▼
    public void OnQuizTimeout() {
        if (gameState == null) return;

        // 퀴즈 창 닫기
        if (UIManager.Instance != null) UIManager.Instance.CloseAllPanels();

        // 오답 처리 (시간 초과 메시지)
        ProcessWrongAnswer("시간 초과!");
        SaveGame();
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    // 오답/시간초과 공통 처리 로직
    private void ProcessWrongAnswer(string msg) {
        gameState.attemptsLeft--; // 목숨 차감

        if (gameState.attemptsLeft <= 0) {
            StartCoroutine(ProcessGameOverRoutine(msg));
        } else {
            if (UIManager.Instance != null) {
                UIManager.Instance.UpdateUI();
                UIManager.Instance.ShowInteractionMessage($"{msg} 남은 기회: {gameState.attemptsLeft}번");
            }
        }
    }

    private IEnumerator ProcessCorrectAnswerRoutine() {
        if (UIManager.Instance != null) UIManager.Instance.ShowInteractionMessage("정답입니다");
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("WinScene");
    }

    private IEnumerator ProcessGameOverRoutine(string msg = "탈락입니다") {
        if (UIManager.Instance != null) {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowInteractionMessage(msg); // "시간 초과!" 또는 "탈락입니다"
        }
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("WinScene");
    }

    public void ChangeFloor(int floorNumber) {
        if (gameState == null) return;
        int totalFloors = gameState.gameFloors.Count;

        if (floorNumber < 1 || floorNumber > totalFloors) {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowInteractionMessage($"존재하지 않는 층입니다.");
            return;
        }

        gameState.currentFloor = floorNumber;
        if (UIManager.Instance != null) {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowInteractionMessage($"{floorNumber}층에 도착했습니다.");
        }
        SaveGame();
    }

    public void SaveGame() {
        if (gameState == null) return;
        string json = JsonUtility.ToJson(gameState, true);
        File.WriteAllText(saveFilePath, json);
    }

    public List<PlayerRecord> LoadMemos() {
        if (File.Exists(memoFilePath)) {
            try {
                string json = File.ReadAllText(memoFilePath);
                MemosWrapper wrapper = JsonUtility.FromJson<MemosWrapper>(json);
                if (wrapper != null && wrapper.memos != null) return wrapper.memos;
            } catch (Exception) { }
        }
        return new List<PlayerRecord>();
    }

    public void SaveMemoAndExit(string memo, string status) {
        if (gameState == null) return;

        PlayerRecord record = new PlayerRecord {
            playerId = gameState.currentPlayerId,
            status = status,
            memo = memo,
            timestamp = System.DateTime.Now.ToString()
        };

        if (gameState.playerHistory == null) gameState.playerHistory = new List<PlayerRecord>();
        gameState.playerHistory.Add(record);

        List<PlayerRecord> allMemos = LoadMemos();
        allMemos.Add(record);
        MemosWrapper wrapper = new MemosWrapper { memos = allMemos };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(memoFilePath, json);

        SaveGame();
    }

    public void ResetGameData() {
        if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
        if (File.Exists(memoFilePath)) File.Delete(memoFilePath);
        InitializeNewGame();
        SceneManager.LoadScene("GameScene");
        Debug.Log("게임 리셋 완료!");
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F5)) ResetGameData();
    }

    [System.Serializable]
    private class MemosWrapper { public List<PlayerRecord> memos = new List<PlayerRecord>(); }

    private void OnApplicationQuit() {
        if (gameState != null) SaveGame();
    }
}