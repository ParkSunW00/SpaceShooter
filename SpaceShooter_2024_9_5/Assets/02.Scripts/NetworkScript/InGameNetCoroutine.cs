using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InGameNetCoroutine : MonoBehaviour
{
    string BestScoreUrl = "";
    string MyGoldUrl = "";
    string UpdateFloorUrl = "";

    GameMgr m_RefGameMgr = null;

    // Start is called before the first frame update
    void Start()
    {
        BestScoreUrl = "http://xxxxxx.dothome.co.kr/xxxxxx/UpdateBScore.php";
        MyGoldUrl = "http://xxxxxx.dothome.co.kr/xxxxxx/UpdateMyGold.php";
        UpdateFloorUrl = "http://xxxxxx.dothome.co.kr/xxxxxx/UpdateFloor.php";
    }

    public void ReadyNetMgr(MonoBehaviour a_CurMgr)
    {
        // as �����ڴ� �־��� ��ü�� ������ Ÿ������ ĳ������ �������� Ȯ���ϰ�,
        // �����ϸ� �ش� Ÿ������ ��ȯ�ϸ�, �Ұ����ϸ� null�� ��ȯ�մϴ�.
        m_RefGameMgr = a_CurMgr as GameMgr;
    }

    public IEnumerator UpdateScoreCo()
    {
        if(GlobalValue.g_Unique_ID == "")  //���������� �α��� �Ǿ� ���� �ʴٸ�...
            yield break;    //�ڷ�ƾ �Լ��� ���� ������...

        WWWForm form = new WWWForm();   
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_score", GlobalValue.g_BestScore);

        //Ÿ�Ӿƿ� ���� (�� ������ ����, ��: 3��)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        UnityWebRequest a_www = UnityWebRequest.Post(BestScoreUrl, form);

        a_www.SendWebRequest(); //������ ��û

        //������ ���ų� Ÿ�Ӿƿ��� �߻��� ������ ���
        while( !a_www.isDone && !isTimeOut )
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;  // ���� �����ӱ��� ���
        }//while( !a_www.isDone && !isTimeOut )

        //Ÿ�Ӿƿ� ó��
        if(isTimeOut == true)
        {
            a_www.Abort(); //��û�� �ߴ�
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        if(a_www.error == null)  //������ ���� �ʾ��� �� ����
        {
            //Debug.Log("UpdateSuccess~");
        }
        else
        {
            Debug.Log(a_www.error);
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;

    }//public IEnumerator UpdateScoreCo()

    public IEnumerator UpdateGoldCo()
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_gold", GlobalValue.g_UserGold);

        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        UnityWebRequest a_www = UnityWebRequest.Post(MyGoldUrl, form);

        a_www.SendWebRequest();     //������ ��û

        //������ ���ų� Ÿ�Ӿƿ��� �߻��� ������ ���
        while (!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;  // ���� �����ӱ��� ���
        }//while( !a_www.isDone && !isTimeOut )

        //Ÿ�Ӿƿ� ó��
        if (isTimeOut == true)
        {
            a_www.Abort(); //��û�� �ߴ�
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        if (a_www.error == null)  //������ ���� �ʾ��� �� ����
        {
            //Debug.Log("UpdateSuccess~");
        }
        else
        {
            Debug.Log(a_www.error);
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
    }

    public IEnumerator UpdateFloorCo()
    {
        if(GlobalValue.g_Unique_ID == "")
            yield break;

        //--- JSON �����...
        FloorInfo a_FInfo = new FloorInfo();
        a_FInfo.CurFloor  = GlobalValue.g_CurFloorNum;
        a_FInfo.BestFloor = GlobalValue.g_BestFloor;
        string a_StrJson = JsonUtility.ToJson(a_FInfo);
        //--- JSON �����...

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_floor", a_StrJson, System.Text.Encoding.UTF8);

        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        UnityWebRequest a_www = UnityWebRequest.Post(UpdateFloorUrl, form);

        a_www.SendWebRequest();     //������ ��û

        //������ ���ų� Ÿ�Ӿƿ��� �߻��� ������ ���
        while (!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;  // ���� �����ӱ��� ���
        }//while( !a_www.isDone && !isTimeOut )

        //Ÿ�Ӿƿ� ó��
        if (isTimeOut == true)
        {
            a_www.Abort(); //��û�� �ߴ�
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        if (a_www.error == null)  //������ ���� �ʾ��� �� ����
        {
            //Debug.Log("UpdateSuccess~");
        }
        else
        {
            Debug.Log(a_www.error);
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;

    }// public IEnumerator UpdateFloorCo()
}
