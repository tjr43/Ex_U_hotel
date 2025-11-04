using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq; // .LastOrDefault()를 위해 추가

// [수정됨] 이 파일은 UIManager와 TransitionManager를 직접 호출합니다.
// UIManager.cs와 TransitionManager.cs가 같은 폴더에 있어야 합니다.
// using StarterAssets; // 만약 StarterAssets 폴더로 옮겼다면 이 줄의 주석을 해제하세요.

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState gameState;

    private string saveFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 GameManager는 파괴되지 않음

        saveFilePath = Path.Combine(Application.persistentDataPath, "invitation.json");
        LoadGameAndInitialize();
    }

    private void LoadGameAndInitialize()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            gameState = JsonUtility.FromJson<GameState>(json);

            // JsonUtility는 List<Trap>의 내부 데이터를 제대로 복구하지 못할 수 있습니다.
            // GameDataInitializer의 데이터로 층 정보를 다시 채워주는 것이 안전합니다.
            // [오류 수정] CreateInitialState() -> createInitialState() (소문자 c)
            var initialState = GameDataInitializer.createInitialState();
            gameState.gameFloors = initialState.gameFloors;
        }

        if (gameState == null)
        {
            // [오류 수정] CreateInitialState() -> createInitialState() (소문자 c)
            gameState = GameDataInitializer.createInitialState();
            SaveGame(); // 첫 시작 시 새 파일 저장
        }

        // 플레이어 기록이 null일 경우 초기화
        if (gameState.playerHistory == null)
        {
            gameState.playerHistory = new System.Collections.Generic.List<PlayerRecord>();
        }
        if (gameState.allPlayers == null)
        {
            gameState.allPlayers = new System.Collections.Generic.List<string>();
        }
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(gameState);
        File.WriteAllText(saveFilePath, json);
    }

    // --- 원작 Servlet 로직 ---

    public void StartNewPlayer(string playerName)
    {
        if (gameState.allPlayers.Contains(playerName))
        {
            // [수정됨] UIManager로 메시지 전송
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("한번 방문하신 분은 재입장하실 수 없습니다.");
            }
            return;
        }

        gameState.allPlayers.Add(playerName);
        gameState.currentPlayerId = playerName;
        gameState.currentFloor = 1;
        gameState.attemptsLeft = 2;

        // [수정됨] UIManager로 UI 갱신
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
        }

        SceneManager.LoadScene("GameScene"); // (씬 이름이 GameScene이라고 가정)
    }

    public void SubmitAnswer(string answer)
    {
        int currentFloor = gameState.currentFloor;
        Floor floor = gameState.gameFloors[currentFloor - 1];

        if (answer.Equals(floor.traps[0].answer, System.StringComparison.OrdinalIgnoreCase))
        {
            // 정답
            gameState.AddClearedFloor(gameState.currentPlayerId, currentFloor);

            // [수정됨] UIManager로 피드백 전송
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("정답입니다! 1층으로 돌아갑니다.");
                gameState.currentFloor = 1; // 1층으로 이동
                UIManager.Instance.UpdateUI();
                UIManager.Instance.HideQuizPanel(); // 퀴즈 창 닫기
            }
        }
        else
        {
            // 오답
            gameState.attemptsLeft--;

            if (gameState.attemptsLeft > 0)
            {
                // [수정됨] UIManager로 피드백 전송
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMessage($"틀렸습니다! 1층으로 돌아갑니다. (남은 기회: {gameState.attemptsLeft})");
                    gameState.currentFloor = 1; // 1층으로 이동
                    UIManager.Instance.UpdateUI();
                    UIManager.Instance.HideQuizPanel(); // 퀴즈 창 닫기
                }
            }
            else
            {
                // 게임 오버
                SceneManager.LoadScene("GameOverScene");
            }
        }
    }

    public void ChangeFloor(int newFloor)
    {
        if (newFloor < 1 || newFloor > 30) // 원작 30층 기준
        {
            // [수정됨] UIManager로 피드백 전송
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("유효하지 않은 층입니다. (1~30층)");
            }
            return;
        }

        if (gameState.IsFloorCleared(gameState.currentPlayerId, newFloor))
        {
            // [수정됨] UIManager로 피드백 전송
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("이미 클리어한 층입니다.");
            }
            return;
        }

        gameState.currentFloor = newFloor;

        if (newFloor == 17) // 17층 즉사 로직
        {
            SceneManager.LoadScene("GameOverScene");
            return;
        }

        // [수정됨] UIManager로 UI 갱신
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
            UIManager.Instance.ShowMessage($"{newFloor}층으로 이동했습니다.");
        }
    }

    public void SaveMemoAndExit(string memo, string status)
    {
        int floorReached = (status == "success") ? 22 : gameState.currentFloor; // 원작 22층 클리어 기준
        PlayerRecord newRecord = new PlayerRecord(gameState.currentPlayerId, floorReached, memo, status);
        gameState.playerHistory.Add(newRecord);

        SaveGame();
        SceneManager.LoadScene("GoodbyeScene");
    }
}

