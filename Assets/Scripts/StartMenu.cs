using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro InputField를 사용하기 위해 필수입니다.

// 1. 클래스 이름이 "StartMenu"로 파일 이름("StartMenu.cs")과 일치해야 합니다.
// 2. ": MonoBehaviour" 부분이 빠지면 안 됩니다.
public class StartMenu : MonoBehaviour
{
    // 1단계에서 만든 이름 입력창을 유니티 에디터에서 연결합니다.
    public TMP_InputField playerNameInputField;

    // "게임 시작" 버튼을 눌렀을 때 호출될 함수 (유니티 OnClick()에 연결)
    public void OnStartGameButtonPressed() {
        string playerName = playerNameInputField.text;

        // 이름이 비어있으면 "Player"로 기본값 설정
        if (string.IsNullOrEmpty(playerName)) {
            playerName = "Player";
        }

        // GameManager를 찾아 이름을 설정하고 GameScene으로 이동
        // (GameManager.Instance.StartGame() 함수를 직접 호출)
        if (GameManager.Instance != null) {
            // GameManager의 StartGame 함수가 이름 설정과 씬 이동을 모두 처리합니다.
            GameManager.Instance.StartGame(playerName);
        } else {
            Debug.LogError("GameManager.Instance가 없습니다! GameManager가 씬에 있는지 확인하세요.");
            // GameManager가 없다면 수동으로 씬 이동 (비상시)
            // PlayerPrefs.SetString("PlayerName", playerName); // 임시 저장
            // SceneManager.LoadScene("GameScene");
        }
    }
}