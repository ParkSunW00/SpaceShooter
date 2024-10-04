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

    //��ŷ ���� ������ Ÿ��
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
            RestoreTimer = 3.0f; //��ŷ ���� Ÿ�̸�
            NetworkMgr.Inst.PushPacket(PacketType.GetRankingList);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_RefLobbyMgr == null)
            return;

#if AutoRestore
        //--- �ڵ� ��ŷ ������ ���
        RestoreTimer -= Time.deltaTime;
        if(RestoreTimer <= 0.0f)
        {
            NetworkMgr.Inst.PushPacket(PacketType.GetRankingList);
            RestoreTimer = 10.0f; //�ֱ�
        }
        //--- �ڵ� ��ŷ ������ ���
#else
        if (0.0f < RestoreTimer)
            RestoreTimer -= Time.deltaTime;
#endif
    }

    public IEnumerator GetRankListCo()
    {
        if(GlobalValue.g_Unique_ID == "")
            yield break;    //�α��� ���� ���¶�� �׳� ����

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);

        //Ÿ�Ӿƿ� ����(�� ������ ����, ��: 3��)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        UnityWebRequest a_www = UnityWebRequest.Post(GetRankListUrl, form);

        a_www.SendWebRequest(); //���� ��û

        //������ ���ų� Ÿ�Ӿƿ��� �߻��� ������ ���
        while(!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;  //���� �����ӱ��� ���
        }//while(!a_www.isDone && !isTimeOut)

        //Ÿ�Ӿƿ� ó��
        if(isTimeOut == true)
        {
            a_www.Abort();  //��û�� �ߴ�
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        if (a_www.error == null) //������ ���� �ʾҴٸ�...
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string a_ReStr = enc.GetString(a_www.downloadHandler.data);

            if (a_ReStr.Contains("Get_Rank_List_Success~") == true)
            {
                a_ReStr = a_ReStr.Replace("\nGet_Rank_List_Success~", "");

                RecRankList_MyRank(a_ReStr); //������ ǥ���ϴ� �Լ��� ȣ��
            }
            else
            {
                if (m_RefLobbyMgr != null)
                    m_RefLobbyMgr.MessageOnOff("���� �ҷ����� ���� ��� �� �ٽ� �õ��� �ּ���.");
            }
        }//if(a_www.error == null) //������ ���� �ʾҴٸ�...
        else
        {
            if (m_RefLobbyMgr != null)
                m_RefLobbyMgr.MessageOnOff("���� �ҷ����� ���� ��� �� �ٽ� �õ��� �ּ���.");
            Debug.Log(a_www.error);
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
    }   

    void RecRankList_MyRank(string strJson)
    {
        if (strJson.Contains("RkList") == false)
            return;

        //Json ���� �Ľ�
        RkRootInfo a_RkList = JsonUtility.FromJson<RkRootInfo>(strJson);

        if (a_RkList == null)
            return;

        if (m_RefLobbyMgr != null)
            m_RefLobbyMgr.RefreshRankUI(a_RkList);

        //Debug.Log(strJson);
    }//void RecRankList_MyRank(string strJson)
}
