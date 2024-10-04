using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class UserInfo
{
    public string user_id;
    public string nick_name;
    public int best_score;
}

[System.Serializable]
public class RkRootInfo
{
    public UserInfo[] RkList;
    public int my_rank;
}

public class LobbyNetCoroutine : MonoBehaviour
{
    string GetRankListUrl = "";

    //랭킹 갱신 딜레이 타임
    [HideInInspector] public float RestoreTimer = 0.0f;

    LobbyMgr m_RefLobbyMgr = null;

    // Start is called before the first frame update
    void Start()
    {
        GetRankListUrl = "http://xxxxxx.dothome.co.kr/xxxxxx/Get_ID_Rank.php";
    }

    public void ReadyNetMgr(MonoBehaviour a_CurMgr)
    {
        m_RefLobbyMgr = a_CurMgr as LobbyMgr;

        if (m_RefLobbyMgr != null)
        {
            RestoreTimer = 3.0f; //랭킹 갱신 타이머
            NetworkMgr.Inst.PushPacket(PacketType.GetRankingList);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_RefLobbyMgr == null)
            return;

#if AutoRestore
        //--- 자동 랭킹 갱신인 경우
        RestoreTimer -= Time.deltaTime;
        if(RestoreTimer <= 0.0f)
        {
            NetworkMgr.Inst.PushPacket(PacketType.GetRankingList);
            RestoreTimer = 10.0f; //주기
        }
        //--- 자동 랭킹 갱신인 경우
#else
        if (0.0f < RestoreTimer)
            RestoreTimer -= Time.deltaTime;
#endif
    }

    public IEnumerator GetRankListCo()
    {
        if(GlobalValue.g_Unique_ID == "")
            yield break;    //로그인 실패 상태라면 그냥 리턴

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);

        //타임아웃 설정(초 단위로 설정, 예: 3초)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        UnityWebRequest a_www = UnityWebRequest.Post(GetRankListUrl, form);

        a_www.SendWebRequest(); //응답 요청

        //응답이 오거나 타임아웃이 발생할 때까지 대기
        while(!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;  //다음 프레임까지 대기
        }//while(!a_www.isDone && !isTimeOut)

        //타임아웃 처리
        if(isTimeOut == true)
        {
            a_www.Abort();  //요청을 중단
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        if (a_www.error == null) //에러가 나지 않았다면...
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string a_ReStr = enc.GetString(a_www.downloadHandler.data);

            if (a_ReStr.Contains("Get_Rank_List_Success~") == true)
            {
                a_ReStr = a_ReStr.Replace("\nGet_Rank_List_Success~", "");

                RecRankList_MyRank(a_ReStr); //점수를 표시하는 함수를 호출
            }
            else
            {
                if (m_RefLobbyMgr != null)
                    m_RefLobbyMgr.MessageOnOff("순위 불러오기 실패 잠시 후 다시 시도해 주세요.");
            }
        }//if(a_www.error == null) //에러가 나지 않았다면...
        else
        {
            if (m_RefLobbyMgr != null)
                m_RefLobbyMgr.MessageOnOff("순위 불러오기 실패 잠시 후 다시 시도해 주세요.");
            Debug.Log(a_www.error);
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
    }   

    void RecRankList_MyRank(string strJson)
    {
        if (strJson.Contains("RkList") == false)
            return;

        //Json 파일 파싱
        RkRootInfo a_RkList = JsonUtility.FromJson<RkRootInfo>(strJson);

        if (a_RkList == null)
            return;

        if (m_RefLobbyMgr != null)
            m_RefLobbyMgr.RefreshRankUI(a_RkList);

        //Debug.Log(strJson);
    }//void RecRankList_MyRank(string strJson)
}
