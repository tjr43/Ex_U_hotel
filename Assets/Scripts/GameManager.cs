using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [HideInInspector] public GameState gameState;

    private const int TOTAL_FLOORS = 30;
    private const int INSTANT_DEFEAT_FLOOR = 17;
    private const int FINAL_FLOOR = 30;
    private const string SAVE_FILE_NAME = "/invitation.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않게 유지

        // 초기 씬 로드는 Start 씬에서 이루어져야 합니다.
    }

    // 이 함수는 StartScene에서만 호출됩니다.
    public void LoadGameAndInitialize()
    {
        if (LoadGame())
        {
            Debug.Log("Game state loaded from file.");
        }
        else
        {
            gameState = GameDataInitializer.CreateInitialState();
            Debug.Log("New game state created.");
        }
    }

    // --- 게임 시작 및 로드 로직 ---
    public bool LoadGame()
    {
        string path = Application.persistentDataPath + SAVE_FILE_NAME;
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                // GameState를 역직렬화할 때 Dictionary<string, List<int>>를 직접 다루기 어렵기 때문에
                // Newtonsoft.Json 같은 라이브러리를 사용하거나 별도의 헬퍼 클래스를 써야 합니다.
                // 여기서는 Unity의 JsonUtility의 한계로 인해 임시로 단순 직렬화만 사용합니다.
                gameState = JsonUtility.FromJson<GameState>(json);
                if (gameState != null) return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game file: {e.Message}");
            }
        }
        return false;
    }

    public void StartNewPlayer(string playerName)
    {
        if (gameState.allPlayers.Contains(playerName))
        {
            // UIManager를 통해 Start 씬에서 경고 메시지 표시
            Debug.LogWarning($"Player {playerName} already exists.");
            return;
        }

        gameState.allPlayers.Add(playerName);
        gameState.currentPlayerId = playerName;
        gameState.currentFloor = 1;
        gameState.attemptsLeft = 2;

        // 게임 씬으로 전환
        SceneManager.LoadScene("GameScene");
    }

    // --- 게임 플레이 로직 ---
    public void ChangeFloor(int newFloor)
    {
        if (newFloor < 1 || newFloor > TOTAL_FLOORS)
        {
            UIManager.Instance.ShowMessage("유효하지 않은 층입니다. 1층부터 30층 사이의 번호를 입력해주세요.");
            return;
        }

        if (gameState.currentPlayerId != null && gameState.IsFloorCleared(gameState.currentPlayerId, newFloor))
        {
            UIManager.Instance.ShowMessage("이미 클리어한 층입니다. 다른 층으로 이동해주세요.");
            return;
        }

        gameState.currentFloor = newFloor;

        if (newFloor == INSTANT_DEFEAT_FLOOR)
        {
            UIManager.Instance.ShowMessage("17층에 들어서자 불길한 기운이 당신을 덮칩니다...");
            Invoke("TriggerGameOver", 3f);
            return;
        }

        // 퀴즈/로비 메시지 표시 및 UI 업데이트
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.OnHideModalButton(null); // 이동 후 팝업 닫기
        }
    }

    public void SubmitAnswer(string answer)
    {
        int currentFloor = gameState.currentFloor;

        if (currentFloor == 1 || currentFloor == 7 || gameState.gameFloors.Count <= currentFloor - 1 || gameState.gameFloors[currentFloor - 1].traps.Count == 0)
        {
            UIManager.Instance.ShowMessage("이 층에서는 정답을 제출할 수 없습니다.");
            return;
        }

        string correctAnswer = gameState.gameFloors[currentFloor - 1].traps[0].answer;

        if (answer.Trim().Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase))
        {
            HandleCorrectAnswer();
        }
        else
        {
            HandleWrongAnswer();
        }
    }

    private void HandleCorrectAnswer()
    {
        // 층 클리어 기록
        if (!gameState.completedFloorsByPlayer.ContainsKey(gameState.currentPlayerId))
        {
            gameState.completedFloorsByPlayer[gameState.currentPlayerId] = new List<int>();
        }
        gameState.completedFloorsByPlayer[gameState.currentPlayerId].Add(gameState.currentFloor);

        if (gameState.currentFloor == FINAL_FLOOR)
        {
            SceneManager.LoadScene("WinScene");
        }
        else
        {
            UIManager.Instance.ShowMessage("정답입니다! 1층으로 돌아갑니다.");
            gameState.currentFloor = 1;
            UIManager.Instance.UpdateUI();
        }
    }

    private void HandleWrongAnswer()
    {
        gameState.attemptsLeft--;

        if (gameState.attemptsLeft > 0)
        {
            UIManager.Instance.ShowMessage($"틀렸습니다! 기회를 1회 잃고 1층으로 돌아갑니다. 남은 기회: {gameState.attemptsLeft}");
            gameState.currentFloor = 1;
            UIManager.Instance.UpdateUI();
        }
        else
        {
            UIManager.Instance.ShowMessage("기회를 모두 소진했습니다!");
            Invoke("TriggerGameOver", 2f);
        }
    }

    private void TriggerGameOver()
    {
        SceneManager.LoadScene("GameOverScene");
    }

    // --- 게임 종료 및 저장 로직 ---
    public void SaveMemoAndExit(string memo, string status)
    {
        int floorReached = (status.Equals("success")) ? FINAL_FLOOR : gameState.currentFloor;
        PlayerRecord newRecord = new PlayerRecord(gameState.currentPlayerId, floorReached, memo, status);
        gameState.playerHistory.Add(newRecord);

        SaveGame();

        SceneManager.LoadScene("GoodbyeScene");
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(gameState, true); // true는 가독성 좋게 출력
        string path = Application.persistentDataPath + SAVE_FILE_NAME;

        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"Game Saved successfully to {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }
}
