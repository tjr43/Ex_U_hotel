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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "gameState.json");
        memoFilePath = Path.Combine(Application.persistentDataPath, "memos.json");
    }

    public void StartGame(string playerName)
    {
        InitializeNewGame();
        if (gameState != null) gameState.currentPlayerId = playerName;
        SceneManager.LoadScene("GameScene");
    }

    private void InitializeNewGame()
    {
        GameState initialState = GameDataInitializer.createInitialState();
        gameState = new GameState(1, "Player", initialState.gameFloors, 2);
        gameState.playerHistory = LoadMemos();
    }

    public void LoadGameOrCreateNew()
    {
        if (File.Exists(saveFilePath)) LoadGameState();
        if (gameState == null || gameState.gameFloors == null || gameState.gameFloors.Count == 0) InitializeNewGame();
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
            if (gameState.playerHistory == null) gameState.playerHistory = LoadMemos();
        }
    }

    // ▼▼▼ [수정됨] 정답/탈락 시 화면 가리는 패널 닫기 ▼▼▼
    public void SubmitAnswer(string answer)
    {
        if (gameState == null) return;

        int currentFloorIdx = gameState.currentFloor - 1;
        if (currentFloorIdx < 0 || currentFloorIdx >= gameState.gameFloors.Count) return;

        Floor floorData = gameState.gameFloors[currentFloorIdx];

        if (floorData.traps != null && floorData.traps.Count > 0)
        {
            Trap trap = floorData.traps[0];

            // [정답 처리]
            if (answer.Trim() == trap.answer)
            {
                // 1. 화면을 가리고 있는 퀴즈 패널부터 닫습니다! (핵심)
                if (UIManager.Instance != null) UIManager.Instance.CloseAllPanels();

                // 2. 그 다음 메시지를 띄우고 코루틴 시작
                StartCoroutine(ProcessCorrectAnswerRoutine());
            }
            // [오답 처리]
            else
            {
                gameState.attemptsLeft--;

                // 목숨 소진 (탈락)
                if (gameState.attemptsLeft <= 0)
                {
                    // 1. 화면을 가리고 있는 퀴즈 패널 닫기 (핵심)
                    if (UIManager.Instance != null) UIManager.Instance.CloseAllPanels();

                    // 2. 탈락 메시지 띄우고 코루틴 시작
                    StartCoroutine(ProcessGameOverRoutine());
                }
                else
                {
                    // 기회가 남았을 때는 패널을 닫지 않고 메시지만 띄움 (다시 풀어야 하니까)
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.UpdateUI();
                        UIManager.Instance.ShowInteractionMessage($"틀렸습니다! 남은 기회: {gameState.attemptsLeft}번");
                    }
                }
            }
            SaveGame();
        }
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    private IEnumerator ProcessCorrectAnswerRoutine()
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowInteractionMessage("정답입니다");
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("WinScene");
    }

    private IEnumerator ProcessGameOverRoutine()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowInteractionMessage("탈락입니다");
        }
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("WinScene");
    }

    public void ChangeFloor(int floorNumber)
    {
        if (gameState == null) return;
        int totalFloors = gameState.gameFloors.Count;

        if (floorNumber < 1 || floorNumber > totalFloors)
        {
            if (UIManager.Instance != null) UIManager.Instance.ShowInteractionMessage("존재하지 않는 층입니다.");
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
            try
            {
                string json = File.ReadAllText(memoFilePath);
                MemosWrapper wrapper = JsonUtility.FromJson<MemosWrapper>(json);
                if (wrapper != null && wrapper.memos != null) return wrapper.memos;
            }
            catch (Exception e) { Debug.LogWarning("메모 로드 실패: " + e.Message); }
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

        if (gameState.playerHistory == null) gameState.playerHistory = new List<PlayerRecord>();
        gameState.playerHistory.Add(record);

        List<PlayerRecord> allMemos = LoadMemos();
        allMemos.Add(record);
        MemosWrapper wrapper = new MemosWrapper { memos = allMemos };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(memoFilePath, json);

        SaveGame();
    }

    public void ResetGameData()
    {
        if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
        if (File.Exists(memoFilePath)) File.Delete(memoFilePath);
        InitializeNewGame();
        SceneManager.LoadScene("GameScene");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5)) ResetGameData();
    }

    [System.Serializable]
    private class MemosWrapper { public List<PlayerRecord> memos = new List<PlayerRecord>(); }

    private void OnApplicationQuit()
    {
        if (gameState != null) SaveGame();
    }
}