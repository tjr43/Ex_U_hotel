using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic; // List<PlayerRecord>를 위해 추가

// [수정됨] 이 파일은 TransitionManager를 직접 호출합니다.
// TransitionManager.cs가 같은 폴더에 있어야 합니다.
// using StarterAssets; // 만약 StarterAssets 폴더로 옮겼다면 이 줄의 주석을 해제하세요.


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Text Components")]
    public TMP_Text playerNameText;
    public TMP_Text attemptsText;
    public TMP_Text quizRiddleText;
    public TMP_Text quizDescriptionText;
    public TMP_Text messageText; // 게임 메시지 출력용

    [Header("Input Fields")]
    public TMP_InputField floorInput;
    public TMP_InputField answerInput;

    [Header("Buttons")]
    public Button submitButton;
    public Button changeFloorButton;

    [Header("Panel References")]
    public GameObject quizPanel;
    public GameObject memoPanel;
    public GameObject rulesPanel;

    [Header("Memo List Components")]
    public RectTransform memoListContent;
    public GameObject memoItemPrefab; // 메모 항목 프리팹 (이름, 메모 텍스트 포함)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 씬 로드 후 UI 초기화 (GameScene에 붙어있다고 가정)
        // 퀴즈/메모/규칙 패널은 처음에 모두 숨깁니다.
        if (quizPanel != null) quizPanel.SetActive(false);
        if (memoPanel != null) memoPanel.SetActive(false);
        if (rulesPanel != null) rulesPanel.SetActive(false);

        UpdateUI();
    }

    // --- 게임 상태 갱신 ---
    public void UpdateUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameState == null) return;

        GameState state = GameManager.Instance.gameState;

        // 1. 공통 정보 업데이트 (HUD)
        if (playerNameText != null) playerNameText.text = state.currentPlayerId;
        if (attemptsText != null) attemptsText.text = $"남은 기회: {state.attemptsLeft}";

        // 2. 퀴즈 패널이 활성화되어 있다면, 퀴즈 정보 업데이트
        if (quizPanel != null && quizPanel.activeSelf)
        {
            int currentFloor = state.currentFloor;
            if (currentFloor < 1 || currentFloor > state.gameFloors.Count) return;

            Floor currentFloorData = state.gameFloors[currentFloor - 1];
            bool isLobbyOrRest = currentFloor == 1 || currentFloor == 7;

            if (isLobbyOrRest)
            {
                if (quizDescriptionText != null) quizDescriptionText.text = currentFloor == 1 ? "1층 로비" : "7층 휴식 공간";
                if (quizRiddleText != null) quizRiddleText.text = currentFloor == 1 ? "다음 층으로 이동하세요." : "잠시 쉬어가세요.";
            }
            else
            {
                // 퀴즈 층 (미클리어 상태여야 퀴즈 패널이 보임)
                Trap trap = currentFloorData.traps[0];
                if (quizDescriptionText != null) quizDescriptionText.text = $"--- {currentFloor}층 --- [방송] {trap.description}";
                if (quizRiddleText != null) quizRiddleText.text = $"[문제] {trap.riddle}";
            }
        }
    }

    public void ShowMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            // (선택 사항) 몇 초 뒤에 메시지가 사라지게 하려면 Coroutine을 사용하세요.
        }
    }

    // --- 버튼 이벤트 핸들러 (유니티 버튼 OnClick에 연결) ---
    public void OnSubmitAnswerButton()
    {
        if (answerInput == null) return;
        string answer = answerInput.text;
        GameManager.Instance.SubmitAnswer(answer);
        answerInput.text = ""; // 입력 필드 초기화
    }

    public void OnChangeFloorButton()
    {
        if (floorInput == null) return;

        if (int.TryParse(floorInput.text, out int newFloor))
        {
            GameManager.Instance.ChangeFloor(newFloor);
        }
        else
        {
            ShowMessage("유효한 층 번호를 숫자로 입력하세요.");
        }
        floorInput.text = ""; // 입력 필드 초기화
    }

    // --- [추가됨] 1인칭 상호작용을 위한 함수 ---
    public void ShowQuizPanel()
    {
        if (quizPanel != null)
        {
            // 퀴즈 패널을 띄우기 전에 현재 층 정보로 UI를 갱신합니다.
            UpdateUI();
            quizPanel.SetActive(true);

            // TransitionManager를 호출하여 1인칭 모드를 끄고 UI 모드를 켭니다.
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.SetUIMode(true);
            }
        }
    }

    // [추가됨] GameManager가 퀴즈를 푼 후 호출할 함수
    public void HideQuizPanel()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);

            // TransitionManager를 호출하여 UI 모드를 끄고 1인칭 모드를 켭니다.
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.SetUIMode(false);
            }
        }
    }

    // --- [추가됨] 메모/규칙 패널 제어 함수 ---
    public void OnShowMemoButton()
    {
        if (memoPanel != null)
        {
            memoPanel.SetActive(true);
            UpdateMemoList();
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.SetUIMode(true);
            }
        }
    }

    public void OnShowRulesButton()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(true);
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.SetUIMode(true);
            }
        }
    }

    // (버튼 OnClick에 연결)
    public void OnHideModalButton(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
            // 퀴즈 패널도 닫혀있는지 확인 후 1인칭 모드로 복귀
            if ((quizPanel == null || !quizPanel.activeSelf) &&
                (memoPanel == null || !memoPanel.activeSelf) &&
                (rulesPanel == null || !rulesPanel.activeSelf))
            {
                if (TransitionManager.Instance != null)
                {
                    TransitionManager.Instance.SetUIMode(false);
                }
            }
        }
    }

    // --- [추가됨] 메모 리스트 렌더링 ---
    private void UpdateMemoList()
    {
        if (memoListContent == null || memoItemPrefab == null) return;

        // 기존 메모 아이템 제거
        foreach (Transform child in memoListContent)
        {
            Destroy(child.gameObject);
        }

        List<PlayerRecord> history = GameManager.Instance.gameState.playerHistory;
        if (history == null || history.Count == 0) return;

        // 최신순으로 표시 (역순)
        for (int i = history.Count - 1; i >= 0; i--)
        {
            PlayerRecord record = history[i];
            if (record == null || string.IsNullOrEmpty(record.memo)) continue;

            GameObject item = Instantiate(memoItemPrefab, memoListContent);

            // 프리팹 내의 텍스트 컴포넌트들을 찾아서 설정합니다. (구조에 따라 수정 필요)
            TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = record.playerId;
                texts[1].text = $"\"{record.memo}\"";
            }
        }
    }
}

