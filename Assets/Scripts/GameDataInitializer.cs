using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 이 클래스는 GameManager가 게임 상태를 초기화할 때만 사용됩니다.
public static class GameDataInitializer
{
    private static readonly List<Trap> ALL_TRAPS = new List<Trap>
    {
        new Trap("숫자 퍼즐", "2, 4, 6, 8... 다음 숫자는?", "10"),
        new Trap("단어 퍼즐", "개구리가 가장 좋아하는 음식은?", "파리"),
        new Trap("논리 퍼즐", "손은 있지만 잡을 수 없다.", "그림자"),
        new Trap("시간 퍼즐", "오늘의 절반, 내일의 절반은?", "밤"),
        new Trap("단어 퍼즐", "늘 다른 곳을 보지만, 늘 같은 곳에 있는 것은?", "두 눈"),
        new Trap("수수께끼", "입은 있지만 말은 못하고, 몸은 있지만 움직이지 못하는 것은?", "책"),
        new Trap("수학 퍼즐", "4개의 9로 100을 만들어라.", "99+9/9"),
        new Trap("방향 퍼즐", "나는 늘 위를 보지만 늘 내려간다.", "비"),
        new Trap("생각 퍼즐", "나는 늘 너와 함께 있지만, 네가 나를 잡으려 하면 사라진다.", "그림자"),
        new Trap("동물 퍼즐", "낮에는 네 발, 저녁에는 두 발, 밤에는 세 발을 걷는 동물은?", "인간"),
        new Trap("지형 퍼즐", "나는 늘 움직이지만, 절대 움직이지 않는다.", "강"),
        new Trap("단어 퍼즐", "나에게 한 단어를 주면 너에게 한 문장을 돌려줄게.", "책"),
        new Trap("계절 퍼즐", "나는 늘 따뜻하지만, 늘 차갑다.", "겨울"),
        new Trap("물건 퍼즐", "나를 만들 때는 길고, 나를 사용할 때는 짧다.", "양초"),
        new Trap("도구 퍼즐", "나는 늘 먹지만, 결코 배부르지 않다.", "칼"),
        new Trap("과일 퍼즐", "나는 초록색 껍질을 가지고 있고, 빨간 속살을 가지고 있다.", "수박"),
        new Trap("날씨 퍼즐", "나는 늘 젖어있지만, 결코 물에 닿지 않는다.", "구름"),
        new Trap("탈것 퍼즐", "바퀴는 있지만 움직이지 않고, 길은 있지만 걸어가지 않는다.", "기차"),
        new Trap("요리 퍼즐", "나는 늘 작아지지만, 결코 줄어들지 않는다.", "치즈"),
        new Trap("악기 퍼즐", "나는 늘 노래하지만, 결코 말이 없다.", "피아노"),
        new Trap("신체 퍼즐", "나는 늘 머리를 가지고 있지만, 결코 생각하지 않는다.", "빗"),
        new Trap("곤충 퍼즐", "나는 늘 춤을 추지만, 결코 발이 없다.", "나비"),
        new Trap("전자기기 퍼즐", "나는 늘 빛을 발하지만, 결코 타지 않는다.", "전구"),
        new Trap("식물 퍼즐", "나는 늘 잎을 가지고 있지만, 결코 숨을 쉬지 않는다.", "종이"),
        new Trap("음료 퍼즐", "나는 늘 차갑지만, 결코 얼지 않는다.", "탄산수"),
        new Trap("옷 퍼즐", "나는 늘 입고 있지만, 결코 몸에 닿지 않는다.", "그림자"),
        new Trap("과목 퍼즐", "나는 늘 어렵지만, 결코 풀리지 않는다.", "수학"),
        new Trap("상식 퀴즈", "세종대왕이 만든 것은?", "한글"),
        new Trap("과학 퀴즈", "물은 몇 도에서 끓는가?", "100"),
        new Trap("종이 퍼즐", "나는 가벼워서 바람에 날아가지만, 붙잡아도 잡을 수 없는 것은?", "연기")
    };

    public static GameState CreateInitialState()
    {
        // 퀴즈 목록을 랜덤하게 섞습니다.
        var shuffledTraps = ALL_TRAPS.OrderBy(x => Random.value).ToList();

        List<Floor> gameFloors = new List<Floor>();
        gameFloors.Add(new Floor(1, new List<Trap>())); // 1층: 로비 (퀴즈 없음)

        for (int i = 0; i < shuffledTraps.Count; i++)
        {
            int floorNumber = i + 2;

            // 7층은 휴식 공간 (퀴즈 없음)
            if (floorNumber == 7)
            {
                gameFloors.Add(new Floor(7, new List<Trap>()));
            }
            else
            {
                // 나머지 층에 퀴즈를 하나씩 배치
                gameFloors.Add(new Floor(floorNumber, new List<Trap> { shuffledTraps[i] }));
            }
        }

        // 초기 상태 반환 (attemptsLeft: 2)
        return new GameState(1, null, new List<PlayerRecord>(), gameFloors, new List<string>(), 2);
    }
}
