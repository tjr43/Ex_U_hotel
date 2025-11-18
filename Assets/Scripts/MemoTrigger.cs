using UnityEngine;

public class MemoTrigger : MonoBehaviour
{
    private bool isPlayerNear = false;

    // 플레이어가 영역 안에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        // 들어온 물체가 'Player' 태그를 달고 있는지 확인
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            // UIManager를 불러와서 메시지 띄우기
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowInteractionMessage("F를 눌러 메모 확인");
            }
        }
    }

    // 플레이어가 영역 밖으로 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            // 메시지 지우기
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideInteractionMessage();
            }
        }
    }

    private void Update()
    {
        // 플레이어가 근처에 있고 F 키를 눌렀을 때
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnShowMemoButton(); // 기존에 만들어진 메모 패널 열기 함수 호출
                UIManager.Instance.HideInteractionMessage(); // 상호작용 메시지는 이제 숨김
            }
        }
    }
}