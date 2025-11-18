using UnityEngine;

public class rulesTrigger : MonoBehaviour
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
                // 영역을 벗어나면 패널도 같이 닫아주는 것이 안전합니다.
                if (UIManager.Instance.rulesPanel.activeSelf)
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
                // [중요] 현재 패널이 열려있는지 확인합니다.
                // UIManager의 rulesPanel 변수가 public이라 직접 확인 가능합니다.
                bool isOpen = UIManager.Instance.rulesPanel.activeSelf;

                if (isOpen)
                {
                    // 이미 열려있으면 -> 닫기 (게임 모드로 복귀)
                    UIManager.Instance.CloseAllPanels();
                }
                else
                {
                    // 닫혀있으면 -> 열기 (UI 모드로 전환, 화면 회전 멈춤)
                    UIManager.Instance.ShowRulePanel();
                    UIManager.Instance.HideInteractionMessage();
                }
            }
        }
    }
}