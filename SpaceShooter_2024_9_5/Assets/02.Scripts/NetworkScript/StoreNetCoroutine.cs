using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StoreNetCoroutine : MonoBehaviour
{
    string BuyRequestUrl = "";

    DragAndDropMgr m_RefDADMgr = null;

    // Start is called before the first frame update
    void Start()
    {
        BuyRequestUrl = "http://xxxxxx.dothome.co.kr/xxxxxx/Buy_Request.php";
    }

    public void ReadyNetMgr(MonoBehaviour a_CurMgr)
    {
        // as 연산자는 주어진 객체가 지정한 타입으로 캐스팅이 가능한지 확인하고,
        // 가능하면 해당 타입으로 변환하며, 불가능하면 null을 반환합니다.
        m_RefDADMgr = a_CurMgr as DragAndDropMgr;
    }

    public IEnumerator BuyRequestCo()
    {
        //--- JSON 만들기...
        ItemList a_ItList = new ItemList();
        a_ItList.SkList = new int[GlobalValue.g_SkillCount.Length];
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            a_ItList.SkList[i] = GlobalValue.g_SkillCount[i];
        }

        // JSON 문자열로 변환 예) {"SkList":[1,1,0]}
        string a_SvStrJson = JsonUtility.ToJson(a_ItList);
        //Debug.Log(a_SvStrJson);
        //--- JSON 만들기...

        if(string.IsNullOrEmpty(a_SvStrJson) == true)
        {
            if (m_RefDADMgr != null)
                m_RefDADMgr.RecoverItem();
            yield break;    //구매 실패 상태라면 그냥 리턴
        }

        if(string.IsNullOrEmpty(GlobalValue.g_Unique_ID) == true)
        {
            if (m_RefDADMgr != null)
                m_RefDADMgr.RecoverItem();
            yield break;    //구매 실패 상태라면 그냥 리턴
        }

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_gold", GlobalValue.g_UserGold);
        form.AddField("Item_list", a_SvStrJson, System.Text.Encoding.UTF8);

        UnityWebRequest a_www = UnityWebRequest.Post(BuyRequestUrl, form);

        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        if (m_RefDADMgr != null)
            m_RefDADMgr.m_BuyDelayTime = 1.0f;

        a_www.SendWebRequest();     //응답이 올 때까지 대기하기...

        //응답이 오거나 타임아웃이 발생할 때까지 대기
        while(!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null; //다음 프레임까지 대기
        }//while(!a_www.isDone && !isTimeOut)

        //타임아웃 처리
        if(isTimeOut == true)
        {
            a_www.Abort();  //요청을 중단
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            if (m_RefDADMgr != null)
                m_RefDADMgr.RecoverItem();
            yield break;
        }

        if(a_www.error == null)  //에러 나지 않았다면...
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string a_ReStr = enc.GetString(a_www.downloadHandler.data);

            if(a_ReStr.Contains("UpdateSuccess~") == false) //구매 실패시
            {
                if (m_RefDADMgr != null)
                    m_RefDADMgr.RecoverItem();
                Debug.Log(a_ReStr);
            }
        }
        else //구매 실패시
        {
            if (m_RefDADMgr != null)
                m_RefDADMgr.RecoverItem();
            Debug.Log(a_www.error);
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;

        yield return null;  
    }
}
