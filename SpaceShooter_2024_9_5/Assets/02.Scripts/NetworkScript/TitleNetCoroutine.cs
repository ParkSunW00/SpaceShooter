using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleNetCoroutine : MonoBehaviour
{
    string LoginUrl = "";
    string CreateUrl = "";

    Title_Mgr m_RefTitleMgr = null;

    // Start is called before the first frame update
    void Start()
    {
        LoginUrl = "http://xxxxxx.dothome.co.kr/xxxxxx/Login.php";
        CreateUrl = "http://xxxxxx.dothome.co.kr/xxxxxx/CreateAccount.php";
    }

    public void ReadyNetMgr(MonoBehaviour a_CurMgr)
    {
        // as �����ڴ� �־��� ��ü�� ������ Ÿ������ ĳ������ �������� Ȯ���ϰ�,
        // �����ϸ� �ش� Ÿ������ ��ȯ�ϸ�, �Ұ����ϸ� null�� ��ȯ�մϴ�.
        m_RefTitleMgr = a_CurMgr as Title_Mgr;
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public IEnumerator LoginCo(string a_IdStr, string a_PwStr)
    {
        WWWForm form = new WWWForm();

        form.AddField("Input_id", a_IdStr, System.Text.Encoding.UTF8);
        form.AddField("Input_pw", a_PwStr);

        UnityWebRequest a_www = UnityWebRequest.Post(LoginUrl, form);

        //Ÿ�Ӿƿ� ���� (�� ������ ����, ��: 3��)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        a_www.SendWebRequest();    //������ ��û

        //������ ���ų� Ÿ�Ӿƿ��� �߻��� ������ ���
        while(!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;
        }
        //������ ���ų� Ÿ�Ӿƿ��� �߻��� ������ ���

        //Ÿ�Ӿƿ� ó��
        if(isTimeOut == true)
        {
            a_www.Abort();      //��û�� �ߴ�
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        bool IsExit = false;
        if (a_www.error == null) //������ �߻����� ���� ���
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);

            if (sz.Contains("Id does not exist.") == true)
            {
                if(m_RefTitleMgr != null)
                   m_RefTitleMgr.MessageOnOff("���̵� �������� �ʽ��ϴ�.");
                IsExit = true;
            }
            else if (sz.Contains("Password does not match.") == true)
            {
                if (m_RefTitleMgr != null)
                    m_RefTitleMgr.MessageOnOff("��й�ȣ�� ��ġ���� �ʽ��ϴ�.");
                IsExit = true;
            }
            else if (sz.Contains("Login_Success!!") == false)
            {
                if (m_RefTitleMgr != null)
                    m_RefTitleMgr.MessageOnOff("�α��� ����, ��� �� �ٽ� �õ��ϰų� ����� ������ �ּ���.");
                IsExit = true;
            }
            else if (sz.Contains("{\"") == false) //JSON ������ �´��� Ȯ�� �ϴ� �ڵ�
            {
                if (m_RefTitleMgr != null)
                    m_RefTitleMgr.MessageOnOff("������ ������ ���������� �ʽ��ϴ�." + sz);
                IsExit = true;
            }

            if(IsExit == true)
            {
                a_www.Dispose();
                NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
                yield break;    //�ڷ�ƾ �Լ��� ��� ���������� ��ɾ�
            }

            GlobalValue.g_Unique_ID = a_IdStr;  //������ ������ȣ

            //"{\"" ���ڿ� �ε��� �պκ��� ����
            string a_GetStr = sz.Substring(sz.IndexOf("{\""));
            //a_GetStr = a_GetStr.Replace("\nLogin_Success!!", "");
            // "\nLogin_Success!!" ������ �޺κ��� ����
            int endIndex = a_GetStr.IndexOf("\nLogin_Success!!");
            a_GetStr = a_GetStr.Substring(0, endIndex);
            //Debug.Log(a_GetStr); //JSON ���ĸ� �����ؼ� ������

            UserInfoRespon a_Respon = JsonUtility.FromJson<UserInfoRespon>(a_GetStr);

            GlobalValue.g_NickName = a_Respon.nick_name;
            GlobalValue.g_BestScore = a_Respon.best_score;
            GlobalValue.g_UserGold = a_Respon.game_gold;

            //--- ������ �ε��� ����...
            if(string.IsNullOrEmpty(a_Respon.floor_info) == false)
            {
                FloorInfo a_FlooInfo = JsonUtility.FromJson<FloorInfo>(a_Respon.floor_info);
                if(a_FlooInfo != null)
                {
                    GlobalValue.g_CurFloorNum = a_FlooInfo.CurFloor;
                    GlobalValue.g_BestFloor   = a_FlooInfo.BestFloor;
                }
            }
            //--- ������ �ε��� ����...

            //--- ItemList �ε��� ����...
            if (string.IsNullOrEmpty(a_Respon.info) == false)
            {
                ItemList a_ItList = JsonUtility.FromJson<ItemList>(a_Respon.info);

                if(a_ItList != null && a_ItList.SkList != null)
                for(int i = 0; i < a_ItList.SkList.Length; i++)
                {
                    if (GlobalValue.g_SkillCount.Length <= i)
                       continue;

                    GlobalValue.g_SkillCount[i] = a_ItList.SkList[i];
                }
            }//if(string.IsNullOrEmpty(a_Respon.info) == false)
            //--- ItemList �ε��� ����...

            //�α��� �����ÿ�...
            if (m_RefTitleMgr != null)
                m_RefTitleMgr.LoginOkGoLobby(a_IdStr);

        }//if(a_www.error == null) //������ �߻����� ���� ���
        else
        {
            if (m_RefTitleMgr != null)
                m_RefTitleMgr.MessageOnOff(a_www.error);
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;

    }//IEnumerator LoginCo(string a_IdStr, string a_PwStr)

    public IEnumerator CreateAccCo(string a_IdStr, string a_PwStr, string a_NickStr)
    {
        WWWForm form = new WWWForm();

        form.AddField("Input_id", a_IdStr, System.Text.Encoding.UTF8);
        form.AddField("Input_pw", a_PwStr);
        form.AddField("Input_nick", a_NickStr, System.Text.Encoding.UTF8);

        UnityWebRequest a_www = UnityWebRequest.Post(CreateUrl, form);

        //Ÿ�Ӿƿ� ���� 
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        a_www.SendWebRequest();    //������ ��û

        //--- ������ ���ų� Ÿ�Ӿƿ��� �߻��� ������ ���
        while(!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;  //���� �����ӱ��� ���
        }
        //--- ������ ���ų� Ÿ�Ӿƿ��� �߻��� ������ ���

        //--- Ÿ�Ӿƿ� ó��
        if(isTimeOut == true)
        {
            a_www.Abort();  //��û�� �ߴ�
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }
        //--- Ÿ�Ӿƿ� ó��

        if (a_www.error == null)  //������ ���� ��
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);

            if (m_RefTitleMgr != null)
            {
                if (sz.Contains("Create Success.") == true)
                {
                    m_RefTitleMgr.IdInputField.text = a_IdStr;
                    m_RefTitleMgr.PwInputField.text = a_PwStr;
                    m_RefTitleMgr.MessageOnOff("���� ����");
                }
                else if (sz.Contains("ID does exist.") == true)
                    m_RefTitleMgr.MessageOnOff("�ߺ��� ID�� �����մϴ�.");
                else if (sz.Contains("Nickname does exist.") == true)
                    m_RefTitleMgr.MessageOnOff("�ߺ��� ������ �����մϴ�.");
                else
                    m_RefTitleMgr.MessageOnOff(sz);
            }//if (m_RefTitleMgr != null)
        }
        else
        {
            if (m_RefTitleMgr != null)
                m_RefTitleMgr.MessageOnOff("���� ���� : " + a_www.error);
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;

    }//IEnumerator CreateActCo(string a_IdStr, string a_PwStr, string a_NickStr)
}
