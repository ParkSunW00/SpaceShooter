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
        // as �����ڴ� �־��� ��ü�� ������ Ÿ������ ĳ������ �������� Ȯ���ϰ�,
        // �����ϸ� �ش� Ÿ������ ��ȯ�ϸ�, �Ұ����ϸ� null�� ��ȯ�մϴ�.
        m_RefDADMgr = a_CurMgr as DragAndDropMgr;
    }

    public IEnumerator BuyRequestCo()
    {
        //--- JSON �����...
        ItemList a_ItList = new ItemList();
        a_ItList.SkList = new int[GlobalValue.g_SkillCount.Length];
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
        {
            a_ItList.SkList[i] = GlobalValue.g_SkillCount[i];
        }

        // JSON ���ڿ��� ��ȯ ��) {"SkList":[1,1,0]}
        string a_SvStrJson = JsonUtility.ToJson(a_ItList);
        //Debug.Log(a_SvStrJson);
        //--- JSON �����...

        if(string.IsNullOrEmpty(a_SvStrJson) == true)
        {
            if (m_RefDADMgr != null)
                m_RefDADMgr.RecoverItem();
            yield break;    //���� ���� ���¶�� �׳� ����
        }

        if(string.IsNullOrEmpty(GlobalValue.g_Unique_ID) == true)
        {
            if (m_RefDADMgr != null)
                m_RefDADMgr.RecoverItem();
            yield break;    //���� ���� ���¶�� �׳� ����
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

        a_www.SendWebRequest();     //������ �� ������ ����ϱ�...

        //������ ���ų� Ÿ�Ӿƿ��� �߻��� ������ ���
        while(!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null; //���� �����ӱ��� ���
        }//while(!a_www.isDone && !isTimeOut)

        //Ÿ�Ӿƿ� ó��
        if(isTimeOut == true)
        {
            a_www.Abort();  //��û�� �ߴ�
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            if (m_RefDADMgr != null)
                m_RefDADMgr.RecoverItem();
            yield break;
        }

        if(a_www.error == null)  //���� ���� �ʾҴٸ�...
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string a_ReStr = enc.GetString(a_www.downloadHandler.data);

            if(a_ReStr.Contains("UpdateSuccess~") == false) //���� ���н�
            {
                if (m_RefDADMgr != null)
                    m_RefDADMgr.RecoverItem();
                Debug.Log(a_ReStr);
            }
        }
        else //���� ���н�
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
