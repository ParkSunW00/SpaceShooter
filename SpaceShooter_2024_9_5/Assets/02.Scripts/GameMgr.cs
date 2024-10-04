using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// <난이도 설정>
// 1, 몬스터는 3층부터 총알을 발사하게 조정
// 2, 몬스터의 총알 발사주기 : 2초(3층)에서 ~ 1초(99층)까지 변하게...
// 3, 층별로 몬스터의 총알 이동속도 증가 :
//          난이도 4층부터 15씩 늘어나도록 800 ~ 3000 증가
// 4, 층별로 몬스터 스폰 마릿수 증가 :
//          8층부터 1마리씩 늘어나도록...
//          시작은 maxMonster = 10; (필드에 활동 가능 몬스터 마릿수 제한) : 10 ~ 25 마리까지
//          시작은 m_MonLimit = 20; (스폰 카운트 마릿수 : 마지막 다이아몬드 스폰) : 20 ~ 30 마리까지
// 5, 높은 층으로 올라 갈수록 높은 가격의 동전을 먹을 수 있게 해서 높은 층으로 올라갈 수록 혜택 주기

public enum GameState
{
    GameIng,
    GameEnd
}

public class GameMgr : MonoBehaviour
{
    public GameState m_GameState = GameState.GameIng;

    //Text UI 항목 연결을 위한 변수
    public Text txtScore;
    //누적 점수를 기록하기 위한 변수
    private int totScore = 0;
    int m_CurScore = 0;     //이번 스테이지에서 얻은 게임점수

    public Button BackBtn;

    //몬스터가 출현할 위치를 담을 배열
    public Transform[] points;
    //몬스터 프리팹을 할당할 변수
    public GameObject monsterPrefab;

    //몬스터를 미리 생성해 저장할 리스트 자료형
    public List<GameObject> monsterPool = new List<GameObject>();

    //몬스터를 발생시킬 주기
    public float createTime = 2.0f;
    //몬스터의 최대 발생 개수
    public int maxMonster = 10;
    //현재 층에서 스폰된 몬스터 카운트 변수
    int m_MonCurNum = 0;
    //현재 층에서 몬스터 최대 스폰 마릿수
    int m_MonLimit = 20;
    //다이아몬드는 조건이 되었을 때 한번만 스폰 시키기 위하여...
    bool m_IsSpawnDiamond = false;
    //현재 층에서 제거한 몬스터 카운트 변수
    [HideInInspector] public int m_CurKillNum = 0;
    //현재 층에서 제거해야 할 몬스터 카운트 변수
    [HideInInspector] public int m_TargetKillNum = 10;

    //게임 종료 여부 변수
    public bool isGameOver = false;

    [Header("--- Game Over ---")]
    public GameObject GameOverPanel = null;
    public Text Title_Text = null;
    public Text Result_Text = null;
    public Button Replay_Btn = null;
    public Button RstLobby_Btn = null;

    //--- 머리 위에 힐(데미지)텍스트 띄우기용 변수 선언
    [Header("--- Heal(Damage) Text ---")]
    public Transform Heal_Canvas = null;
    public Transform Damage_Canvas = null;
    public GameObject DamagePrefab = null;

    [HideInInspector] public GameObject m_CoinItem = null;
    [Header("--- Gold UI ---")]
    public Text m_UserGoldText = null;  //이번 스테이지에서 얻은 골드값 표시 UI
    int m_CurGold = 0;

    [Header("--- Skill Coll Timer ---")]
    GameObject m_SkCoolPrefab = null;
    Transform  m_SkCoolRoot = null;
    public SkInvenNode[] m_SkInvenNode;  //Skill 인벤토리 버튼 연결 변수

    [Header("--- Door Ctrl ---")]
    public Text m_FL_Tm_Text = null;
    public Text m_LastFloorText = null;
    public Text m_DoorOpenText = null;
    float m_Floor_TimeOut = 0.0f;       //이번 층 탈출 시간 타이머
    GameObject[] m_DoorObj = new GameObject[3];
    public static GameObject m_DiamondItem = null;

    public Text m_Help_Text = null;

    PlayerCtrl m_RefHero = null;

    //--- 싱글톤 패턴
    public static GameMgr Inst = null;

    void Awake()
    {
        Inst = this;    
    }
    //--- 싱글톤 패턴

    // Start is called before the first frame update
    void Start()
    {
        NetworkMgr.Inst.ReadyNetworkMgr(this);  //씬마다 추가

        if (IsEndingScene() == true)
            return;

        Time.timeScale = 1.0f;
        GlobalValue.LoadGameData();
        RefreshGameUI();

        DispScore(0);

        NetworkMgr.Inst.PushPacket(PacketType.FloorUpdate);
        //-- 항상 씬에 들어올 때 마다
        //GlobalValue.g_CurFloorNum
        //GlobalValue.g_BestFloor   변화가 생길 것으로 예상하고
        //서버에 층 변경 요청을 한다.

        if (BackBtn != null)
            BackBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Lobby");
            });

        //--- 난이도 층 별로 몬스터 최대 스폰 마리수 증가
        // 8층부터 1마리씩 늘어나도록... 20마리 ~ 30마리까지 스폰 되도록...
        // 시작은 maxMonter = 10; (필드에 활동 가능 몬스터 마리수 제한) : 10 ~ 20마리까지
        // 시작은 m_MonLimit = 20; (이번 스테이지에서 스폰될 마리수) : 20 ~ 30마리까지

        int a_CacMaxMon = GlobalValue.g_CurFloorNum - 7;
        if (a_CacMaxMon < 0)
            a_CacMaxMon = 0;
        a_CacMaxMon = 10 + a_CacMaxMon;
        if (25 < a_CacMaxMon)
            a_CacMaxMon = 25;

        maxMonster = a_CacMaxMon;   //10 ~ 25 마리까지 8층부터 한마리씩 늘어남
        m_MonLimit = 10 + a_CacMaxMon;  //20 ~ 30 마리까지 8층부터 한마리씩 늘어남
        m_TargetKillNum = a_CacMaxMon;  //이번층에서 잡아야 할 몬스터 10 ~ 25 마리까지 증가 되게...
        //--- 난이도 층 별로 몬스터 최대 스폰 마리수 증가

        // Hierarchy 뷰의 SpawnPoint를 찾아 하위에 있는 모든 Transform 컴포넌트를 찾아옴
        points = GameObject.Find("SpawnPoint").GetComponentsInChildren<Transform>();

        //몬스터를 생성해 오브젝트 풀에 저장
        for(int i = 0; i < maxMonster; i++)
        {
            //몬스터 프리팹을 생성
            GameObject monster = (GameObject)Instantiate(monsterPrefab);
            //생성한 몬스터의 이름 설정
            monster.name = "Monster_" + i.ToString();
            //생성한 몬스터를 비활성화
            monster.SetActive(false);
            //생성한 몬스터를 오브젝트 풀에 추가
            monsterPool.Add(monster);
        }

        if(points.Length > 0)
        {
            //몬스터 생성 코루틴 함수 호출
            StartCoroutine(this.CreateMonster());
        }

        //--- GameOver 버튼 처리 코드
        if (Replay_Btn != null)
            Replay_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("scLevel01");
                SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
            });

        if (RstLobby_Btn != null)
            RstLobby_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Lobby");
            });
        //--- GameOver 버튼 처리 코드

        //--- Door 관련 구현 코드
        m_FL_Tm_Text.text = GlobalValue.g_CurFloorNum + "층(도달:" +
                            GlobalValue.g_BestFloor + "층)";

        GameObject a_DoorObj = GameObject.Find("Gate_In_1");
        if(a_DoorObj != null)
            m_DoorObj[0] = a_DoorObj;

        a_DoorObj = GameObject.Find("Gate_Exit_1");
        if(a_DoorObj != null)
        {
            m_DoorObj[1] = a_DoorObj;
            m_DoorObj[1].SetActive(false);
        }

        a_DoorObj = GameObject.Find("Gate_Exit_2");
        if (a_DoorObj != null)
        {
            m_DoorObj[2] = a_DoorObj;
            m_DoorObj[2].SetActive(false);
        }

        if (GlobalValue.g_CurFloorNum <= 1)
            m_DoorObj[0].SetActive(false);

        if(GlobalValue.g_CurFloorNum < GlobalValue.g_BestFloor)
        { //최고 클리어 이하 층이면 그냥 나가는 문을 열어준다.

            ShowDoor();
        }

        m_DiamondItem = Resources.Load("DiamondItem/DiamondPrefab") as GameObject;
        //--- Door 관련 구현 코드

        m_CoinItem = Resources.Load("CoinItem/CoinPrefab") as GameObject;
        m_SkCoolPrefab = Resources.Load("SkCool_Node") as GameObject;
        m_SkCoolRoot   = GameObject.Find("SkillCoolRoot").transform;

        m_RefHero = GameObject.FindObjectOfType<PlayerCtrl>();

    }//void Start()

    //몬스터 생성 코루틴 함수
    IEnumerator CreateMonster()
    {
        //게임 종료 시까지 무한 루프
        while(m_GameState != GameState.GameEnd)
        {
            //몬스터 생성 주기 시간만큰 메인 루프에 양보
            yield return new WaitForSeconds(createTime);

            //플레이어가 사망했을 때 코루틴을 종료해 다음 루틴을 진행하지 않음
            if (m_GameState == GameState.GameEnd)
                yield break;

            //오브젝트 풀의 처음부터 끝까지 순회
            foreach(GameObject monster in monsterPool)
            {
                //비활성화 여부로 사용 가능한 몬스터를 판단
                if(monster.activeSelf == false)
                {
                    //몬스터를 출형시킬 위치의 인덱스값을 추출
                    int idx = Random.Range(1, points.Length);

                    //--- 몬스터 카운트 및 마지막 몬스터 스폰 상태 체크하는 함수
                    if (CheckMonsterCount(idx) == true)
                        break;
                    //--- 몬스터 카운트 및 마지막 몬스터 스폰 상태 체크하는 함수

                    if (m_MonLimit <= m_MonCurNum)
                        break;

                    //몬스터의 출현위치를 설정
                    monster.transform.position = points[idx].position;
                    //몬스터를 활성화함
                    monster.SetActive(true);
                    //오브젝트 풀에서 몬스터 프리팸 하나를 활성화한 후 for 루프를 빠져나감

                    m_MonCurNum++;

                    break;
                }//if(monster.activeSelf == false)

            }//foreach(GameObject monster in monsterPool)

        }//while(m_GameState != GameState.GameEnd)

    }//IEnumerator CreateMonster()

    // Update is called once per frame
    void Update()
    {
        //--- 퀘스트 관련 구현 코드
        if(0.0f < m_Floor_TimeOut)
        {
            m_Floor_TimeOut -= Time.deltaTime;
            m_FL_Tm_Text.text = GlobalValue.g_CurFloorNum + "층(도달:" +
                                GlobalValue.g_BestFloor + "층) / " +
                                m_Floor_TimeOut.ToString("F1");

            if(m_Floor_TimeOut <= 0.0f)
            {
                m_GameState = GameState.GameEnd;
                Time.timeScale = 0.0f;  //일시정지
                GameOverMethod();
            }
        }

        MissionUIUpdate();
        //--- 퀘스트 관련 구현 코드

        //마우스 중앙버튼(휠 클릭)
        if (Input.GetMouseButtonDown(2) == true)
        {
            UseSkill_Key(SkillType.Skill_1); //수류탄 사용
        }

        //--- 단축키 이용으로 스킬 사용하기...
        if( Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1) )
        {
            UseSkill_Key(SkillType.Skill_0);  //30% 힐링 아이템 스킬
        }
        else if( Input.GetKeyDown(KeyCode.Alpha2) ||
                 Input.GetKeyDown(KeyCode.Keypad2) )
        {
            UseSkill_Key(SkillType.Skill_1);  //수류탄 사용
        }
        else if( Input.GetKeyDown(KeyCode.Alpha3) ||
                 Input.GetKeyDown(KeyCode.Keypad3) )
        {
            UseSkill_Key(SkillType.Skill_2);  //보호막 발동
        }
        //--- 단축키 이용으로 스킬 사용하기...
    }

    public void AddGold(int value = 10)
    {
        //이번 스테이지에서 얻은 골드값
        m_CurGold += value;
        if(m_CurGold < 0)
            m_CurGold = 0;

        //로컬에 저장되어 있는 유저 보유 골드값
        if (value < 0)
        {
            GlobalValue.g_UserGold += value;
            if (GlobalValue.g_UserGold < 0)
                GlobalValue.g_UserGold = 0;
        }
        else if (GlobalValue.g_UserGold <= int.MaxValue - value)
            GlobalValue.g_UserGold += value;
        else
            GlobalValue.g_UserGold = int.MaxValue;

        if (m_UserGoldText != null)
            m_UserGoldText.text = "Gold <color=#ffff00>" + GlobalValue.g_UserGold + "</color>";

        //PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
        NetworkMgr.Inst.PushPacket(PacketType.UserGold);
    }

    //점수 누적 및 화면 표시
    public void DispScore(int score)
    {
        //totScore += score;
        //txtScore.text = "SCORE <color=#ff0000>" + totScore.ToString() + "</color>"; 

        m_CurScore += score;
        if(m_CurScore < 0)
            m_CurScore = 0;

        if(score < 0)
        {
            GlobalValue.g_BestScore += score;
            if(GlobalValue.g_BestScore < 0)
                GlobalValue.g_BestScore = 0;
        }
        else if(GlobalValue.g_BestScore <= int.MaxValue - score)
        {
            GlobalValue.g_BestScore += score;
        }
        else
        {
            GlobalValue.g_BestScore = int.MaxValue;
        }

        txtScore.text = "SCORE <color=#ff0000>" + m_CurScore.ToString() +
                "</color> / BEST <color=#ff0000>" +
                GlobalValue.g_BestScore.ToString() + "</color>";

        //PlayerPrefs.SetInt("BestScore", GlobalValue.g_BestScore);
        NetworkMgr.Inst.PushPacket(PacketType.BestScore);
    }

    public static bool IsPointerOverUIObject() //UGUI의 UI들이 먼저 피킹되는지 확인하는 함수
    {
        PointerEventData a_EDCurPos = new PointerEventData(EventSystem.current);

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)

			List<RaycastResult> results = new List<RaycastResult>();
			for (int i = 0; i < Input.touchCount; ++i)
			{
				a_EDCurPos.position = Input.GetTouch(i).position;  
				results.Clear();
				EventSystem.current.RaycastAll(a_EDCurPos, results);
                if (0 < results.Count)
                    return true;
			}

			return false;
#else
        a_EDCurPos.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(a_EDCurPos, results);
        return (0 < results.Count);
#endif
    }//public bool IsPointerOverUIObject() 

    public void GameOverMethod()
    {
        GameOverPanel.SetActive(true);

        Result_Text.text = "NickName\n" + GlobalValue.g_NickName + "\n\n" +
                            "획득 점수\n" + m_CurScore + "\n\n" +
                            "획득 골드\n" + m_CurGold;

        if(SceneManager.GetActiveScene().name == "scLevel02")
        {
            Title_Text.text = "< Game Ending >";
            Result_Text.text = "NickName\n" + GlobalValue.g_NickName + "\n\n" +
                "Made by\n" + "SBS Game Academy" + "\n\n" + "Date\n" + "2024.8.16";
        }
    }

    public void SpawnCoin(Vector3 a_Pos)
    {
        GameObject a_CoinObj = Instantiate(m_CoinItem);
        a_CoinObj.transform.position = a_Pos;
        Destroy(a_CoinObj, 10.0f);      //10초 내에 먹어야 한다.
    }

    public void UseSkill_Key(SkillType a_SkType)
    {
        if (GlobalValue.g_SkillCount[(int)a_SkType] <= 0)
            return;

        if(m_RefHero != null)
           m_RefHero.UseSkill_Item(a_SkType);

        if ((int)a_SkType < m_SkInvenNode.Length)
            m_SkInvenNode[(int)a_SkType].m_SkCountText.text =
                        GlobalValue.g_SkillCount[(int)a_SkType].ToString();
    }

    public void SpawnDamageText(int cont, Vector3 a_SpPos, Color a_Color, bool IsPlayer = false)
    {
        if(Damage_Canvas == null || DamagePrefab == null)
            return;

        GameObject a_DmgObj = Instantiate(DamagePrefab);
        if(IsPlayer == true)
            a_DmgObj.transform.SetParent(Heal_Canvas, false);
        else
            a_DmgObj.transform.SetParent(Damage_Canvas, false);

        DamageText a_DamageTx = a_DmgObj.GetComponent<DamageText>(); 
        if(a_DamageTx != null)
           a_DamageTx.InitState(cont, a_SpPos, a_Color);
    }

    void RefreshGameUI()
    {
        if (m_UserGoldText != null)
            m_UserGoldText.text = "Gold <color=#ffff00>" + GlobalValue.g_UserGold + "</color>";

        for(int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            if(m_SkInvenNode.Length <= i)
                continue;

            m_SkInvenNode[i].InitState((SkillType)i);
        }
    }//void RefreshGameUI()

    public void SkillTimeMethod(float a_Time, float a_Dur)
    {
        GameObject Obj = Instantiate(m_SkCoolPrefab);
        Obj.transform.SetParent(m_SkCoolRoot, false);
        SkCool_NodeCtrl SkNode = Obj.GetComponent<SkCool_NodeCtrl>();
        SkNode.InitState(a_Time, a_Dur);    
    }

    public void ShowDoor()
    {
        int a_Idx = (GlobalValue.g_CurFloorNum % 2) + 1;
        if ((1 <= a_Idx && a_Idx <= 2) && m_DoorObj[a_Idx] != null)
            m_DoorObj[a_Idx].SetActive(true);

        if (m_LastFloorText != null)
            m_LastFloorText.gameObject.SetActive(false);

        if(m_DoorOpenText != null)
            m_DoorOpenText.gameObject.SetActive(true);
    }

    //--- 몬스터 카운트 및 마지막 몬스터 스폰 상태 체크하는 함수
    bool CheckMonsterCount(int idx)
    {
        if(m_IsSpawnDiamond == false && m_TargetKillNum <= m_CurKillNum)
        { //이번층에서 마지막 제거한 몬스터라면 몬스터 대신 다이아몬스를 스폰

            if(GlobalValue.g_BestFloor <= GlobalValue.g_CurFloorNum)
            {  //퀘스트를 수행해야 하는 도달층에서 활동하고 있을 때만

                m_IsSpawnDiamond = true;

                //다이아몬드 스폰
                if(m_DiamondItem != null)
                {
                    GameObject a_DmdObj = Instantiate(m_DiamondItem);
                    a_DmdObj.transform.position = points[idx].position;
                }
                m_Floor_TimeOut = 60.0f;    //60초 타이머 돌리기

                return true;
            }//if(GlobalValue.g_BestFloor <= GlobalValue.g_CurFloorNum)
        }//if(m_IsSpawnDiamond == false && m_TargetKillNum <= m_CurKillNum)

        return false;

    }//bool CheckMonsterCount(int idx)

    void MissionUIUpdate()
    {
        if (m_LastFloorText == null)
            return;

        if (m_LastFloorText.gameObject.activeSelf == false)
            return;

        if(0.0f < m_Floor_TimeOut)
        {
            m_LastFloorText.text = 
                "<color=#00ffff>다이아몬드가 맵 어딘가에 생성되었습니다.</color>";
        }
        else
        {
            m_LastFloorText.text = "<color=#ffff00>(" + m_CurKillNum +
                                    " / " + m_TargetKillNum + " Mon) " +
                                    "최종 100층</color>";
        }
    }//void MissionUIUpdate()

    bool IsEndingScene()
    {
        if(SceneManager.GetActiveScene().name != "scLevel02")
        {
            return false;   //이번에 로딩된 씬이 엔딩씬이 아니라면...
        }

        Time.timeScale = 1.0f;
        m_GameState = GameState.GameIng;

        GlobalValue.LoadGameData();
        RefreshGameUI();

        //처음 실행 후 저장된 스코어 정보 로딩
        DispScore(0);

        if (BackBtn != null)
            BackBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Lobby");
            });

        m_FL_Tm_Text.text = GlobalValue.g_CurFloorNum + "층(도달:" +
                            GlobalValue.g_BestFloor + "층)";

        m_RefHero = GameObject.FindObjectOfType<PlayerCtrl>();

        if (Replay_Btn != null)
            Replay_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Lobby");
            });

        if (RstLobby_Btn != null)
            RstLobby_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Lobby");
            });

        return true;
    }

}
