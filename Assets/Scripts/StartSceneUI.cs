using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환
using TMPro; // TextMeshPro

// 이 스크립트는 StartScene의 Canvas에 붙입니다.
public class StartSceneUI : MonoBehaviour
{
    // 이 슬롯에 '이름 입력창'을 연결합니다.
    public TMP_InputField playerNameInput;

    // 이 슬롯에 '에러 메시지'용 텍스트를 연결합니다.
    public TMP_Text messageText;

    // '입장하기' 버튼의 OnClick() 이벤트가 이 함수를 호출해야 합니다.
    public void OnClickStartButton() {
        if (playerNameInput == null) {
            Debug.LogError("PlayerNameInput이 StartSceneUI에 연결되지 않았습니다!");
            return;
        }

        string playerName = playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName)) {
            if (messageText != null) {
                messageText.text = "이름을 입력해주세요.";
            }
            return;
        }

        // GameManager를 찾아 StartNewPlayer 함수 호출
        // (GameManager는 DontDestroyOnLoad이므로 다른 씬에서 넘어왔다면 이미 존재함)
        if (GameManager.Instance != null) {
            GameManager.Instance.StartNewPlayer(playerName);
        } else {
            // 혹시 StartScene에서 바로 시작할 경우를 대비
            // (이 경우 GameManager가 씬에 있어야 함)
            Debug.LogError("GameManager가 씬에 없습니다!");
        }
    }
}