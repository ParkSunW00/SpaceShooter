using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PacketType
{
    //--- Title
    Login,          //로그인 요청
    CreateAccount,  //계정 생성 요청
    //--- Title

    //--- Lobby
    GetRankingList, //랭킹 받아오기
    NickUpdate,     //닉네임 갱신
    ClearSave,      //서버에 저장된 내용 초기화 하기
    //--- Lobby

    //--- InGame
    BestScore,      //최고점수 갱신 요청
    UserGold,       //유저골드 갱신 요청
    InfoUpdate,     //아이템정보 갱신 요청
    FloorUpdate,    //층 정보 갱신 요청
    //--- InGame

    //--- Store
    BuyRequest,     //상품 구매 요청
    //--- Store
}

[System.Serializable]
public class UserInfoRespon
{
    public string nick_name;
    public int best_score;
    public int game_gold;
    public string floor_info;  //이 필드는 문자열로 유지하고, 추가적으로 파싱이 필요함
    public string info;        //이 필드는 문자열로 유지하고, 추가적으로 파싱이 필요함
}

[System.Serializable]
public class ItemList
{
    public int[] SkList;
}

[System.Serializable]
public class FloorInfo  //층 정보 로딩
{
    public int CurFloor;
    public int BestFloor;
}

public class NetworkMgr : G_Singleton<NetworkMgr>
{
    //--- 서버에 전송할 패킷 처리용 큐 관련 변수
    [HideInInspector] public float m_NetWaitTimer = 0.0f; //Network 대기 상태 변수
    List<PacketType> m_PacketBuff = new List<PacketType>();
    //--- 서버에 전송할 패킷 처리용 큐 관련 변수

    //3초 동안 응답이 없으면 다음 패킷 처리 가능하도록...
    public const float m_Timeout = 3.0f;

    //--- Scene별 컴포넌트 객체
    [HideInInspector] public TitleNetCoroutine  TitleNetCom;
    [HideInInspector] public LobbyNetCoroutine  LobbyNetCom;
    [HideInInspector] public InGameNetCoroutine InGameNetCom;
    [HideInInspector] public StoreNetCoroutine  StoreNetCom;
    //--- Scene별 컴포넌트 객체

    //--- 로그인, 계정생성 매개변수로 넘겨 받을 임시변수
    [HideInInspector] public string m_IdStrBuff = "";
    [HideInInspector] public string m_PwStrBuff = "";
    [HideInInspector] public string m_NickStrBuff = "";
    //--- 로그인, 계정생성 매개변수로 넘겨 받을 임시변수

    protected override void Init() //Awake() 함수 대신 사용
    {
        base.Init();    //부모쪽에 있는 Init() 함수 호출

        TitleNetCom  = gameObject.AddComponent<TitleNetCoroutine>();
        LobbyNetCom  = gameObject.AddComponent<LobbyNetCoroutine>();
        InGameNetCom = gameObject.AddComponent<InGameNetCoroutine>();
        StoreNetCom  = gameObject.AddComponent<StoreNetCoroutine>(); //컴포넌트 추가
    }

    public void ReadyNetworkMgr(MonoBehaviour a_CurMgr) //초기화시 한번 호출
    {
        TitleNetCom.ReadyNetMgr(a_CurMgr);
        LobbyNetCom.ReadyNetMgr(a_CurMgr);
        InGameNetCom.ReadyNetMgr(a_CurMgr);
        StoreNetCom.ReadyNetMgr(a_CurMgr);
    }

    //// Start is called before the first frame update
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    void Update()
    {
        if (0.0f <= m_NetWaitTimer)
            m_NetWaitTimer -= Time.unscaledDeltaTime;

        if(m_NetWaitTimer <= 0.0f)  //지금 패킷 처리 중인 상태가 아니면...
        {
            if(0 < m_PacketBuff.Count)  //대기 패킷이 존재한다면...
            {
                Req_Network();
            }
        }
    }//void Update()

    void Req_Network()  //RequestNetwork
    {
        //--- Title
        if (m_PacketBuff[0] == PacketType.Login) //로그인 요청
            StartCoroutine(TitleNetCom.LoginCo(m_IdStrBuff, m_PwStrBuff));
        else if (m_PacketBuff[0] == PacketType.CreateAccount) //계정 생성 요청
            StartCoroutine(TitleNetCom.CreateAccCo(m_IdStrBuff, m_PwStrBuff, m_NickStrBuff));
        //--- Title

        //--- Lobby
        else if (m_PacketBuff[0] == PacketType.GetRankingList)
            StartCoroutine(LobbyNetCom.GetRankListCo());
        //--- Lobby

        //--- InGame
        else if (m_PacketBuff[0] == PacketType.BestScore)
            StartCoroutine(InGameNetCom.UpdateScoreCo());
        else if (m_PacketBuff[0] == PacketType.UserGold)
            StartCoroutine(InGameNetCom.UpdateGoldCo());
        else if (m_PacketBuff[0] == PacketType.FloorUpdate)
            StartCoroutine(InGameNetCom.UpdateFloorCo());
        //--- InGame

        //--- Store
        else if (m_PacketBuff[0] == PacketType.BuyRequest)
            StartCoroutine(StoreNetCom.BuyRequestCo());
        //--- Store

        m_PacketBuff.RemoveAt(0);
    }

    public void PushPacket(PacketType a_PType)
    {
        bool a_isExist = false;
        for(int i = 0; i < m_PacketBuff.Count; i++)
        {
            if (m_PacketBuff[i] == a_PType) //아직 처리 되지 않은 패킷이 존재하면
                a_isExist = true;
            //또 추가하지 않고 기존 버퍼의 패킷으로 업데이트 한다.
        }

        if (a_isExist == false)
            m_PacketBuff.Add(a_PType);
        //대기 중인 이 타입의 패킷이 없으면 새로 추가한다.

    }//public void PushPacket(PacketType a_PType)
}
