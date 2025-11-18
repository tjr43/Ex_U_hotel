using UnityEngine;

public class MemoTrigger : MonoBehaviour
{
    private bool isPlayerNear = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowInteractionMessage("Press F");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideInteractionMessage();
                // 영역을 벗어나면 패널도 같이 닫기
                if (UIManager.Instance.memoPanel.activeSelf)
                {
                    UIManager.Instance.CloseAllPanels();
                }
            }
        }
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            if (UIManager.Instance != null)
            {
                // [중요] 현재 메모 패널이 열려있는지 확인
                bool isOpen = UIManager.Instance.memoPanel.activeSelf;

                if (isOpen)
                {
                    // 열려있으면 닫기 (다시 움직일 수 있게 됨)
                    UIManager.Instance.CloseAllPanels();
                }
                else
                {
                    // 닫혀있으면 열기
                    UIManager.Instance.ShowMemoPanel();
                    UIManager.Instance.HideInteractionMessage();
                }
            }
        }
    }
}