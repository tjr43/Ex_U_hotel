using System.Collections.Generic;
// TMPro를 사용하여 UI 요소를 만들 예정이므로, UnityEngine.UI를 가져올 필요는 없습니다.

// 유니티의 JsonUtility가 직렬화/역직렬화 할 수 있도록 설정
[System.Serializable]
public class Trap
{
    public string description;
    public string riddle;      // 퀴즈 내용
    public string answer;      // 퀴즈 정답

    public Trap(string description, string riddle, string answer)
    {
        this.description = description;
        this.riddle = riddle;
        this.answer = answer;
    }
}

[System.Serializable]
public class Floor
{
    public int floorNumber;
    public List<Trap> traps;

    public Floor(int floorNumber, List<Trap> traps)
    {
        this.floorNumber = floorNumber;
        this.traps = traps;
    }
}

[System.Serializable]
public class PlayerRecord
{
    public string playerId;
    public int floorReached;
    public string memo;
    public string status; // "success" 또는 "fail"

    public PlayerRecord(string playerId, int floorReached, string memo, string status)
    {
        this.playerId = playerId;
        this.floorReached = floorReached;
        this.memo = memo;
        this.status = status;
    }
}

[System.Serializable]
public class GameState
{
    public int currentFloor;
    public string currentPlayerId;
    public List<PlayerRecord> playerHistory;
    public List<Floor> gameFloors;
    // C#에서는 Set을 JSON으로 직렬화하기 어렵기 때문에, Dictionary<string, List<int>>를 사용합니다.
    public Dictionary<string, List<int>> completedFloorsByPlayer;

    public List<string> allPlayers;
    public int attemptsLeft;

    public GameState(int currentFloor, string currentPlayerId, List<PlayerRecord> playerHistory, List<Floor> gameFloors, List<string> allPlayers, int attemptsLeft)
    {
        this.currentFloor = currentFloor;
        this.currentPlayerId = currentPlayerId;
        this.playerHistory = playerHistory ?? new List<PlayerRecord>();
        this.gameFloors = gameFloors ?? new List<Floor>();
        this.allPlayers = allPlayers ?? new List<string>();
        this.attemptsLeft = attemptsLeft;
        this.completedFloorsByPlayer = new Dictionary<string, List<int>>();
    }

    public bool IsFloorCleared(string playerId, int floor)
    {
        if (completedFloorsByPlayer.TryGetValue(playerId, out var clearedFloors))
        {
            return clearedFloors.Contains(floor);
        }
        return false;
    }
}
