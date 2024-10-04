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
        // as 연산자는 주어진 객체가 지정한 타입으로 캐스팅이 가능한지 확인하고,
        // 가능하면 해당 타입으로 변환하며, 불가능하면 null을 반환합니다.
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

        //타임아웃 설정 (초 단위로 설정, 예: 3초)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        a_www.SendWebRequest();    //서버예 요청

        //응답이 오거나 타임아웃이 발생할 때까지 대기
        while(!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;
        }
        //응답이 오거나 타임아웃이 발생할 때까지 대기

        //타임아웃 처리
        if(isTimeOut == true)
        {
            a_www.Abort();      //요청을 중단
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        bool IsExit = false;
        if (a_www.error == null) //에러가 발생하지 않은 경우
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);

            if (sz.Contains("Id does not exist.") == true)
            {
                if(m_RefTitleMgr != null)
                   m_RefTitleMgr.MessageOnOff("아이디가 존재하지 않습니다.");
                IsExit = true;
            }
            else if (sz.Contains("Password does not match.") == true)
            {
                if (m_RefTitleMgr != null)
                    m_RefTitleMgr.MessageOnOff("비밀번호가 일치하지 않습니다.");
                IsExit = true;
            }
            else if (sz.Contains("Login_Success!!") == false)
            {
                if (m_RefTitleMgr != null)
                    m_RefTitleMgr.MessageOnOff("로그인 실패, 잠시 후 다시 시도하거나 운영진에 문의해 주세요.");
                IsExit = true;
            }
            else if (sz.Contains("{\"") == false) //JSON 형식이 맞는지 확인 하는 코드
            {
                if (m_RefTitleMgr != null)
                    m_RefTitleMgr.MessageOnOff("서버의 응답이 정상적이지 않습니다." + sz);
                IsExit = true;
            }

            if(IsExit == true)
            {
                a_www.Dispose();
                NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
                yield break;    //코루틴 함수를 즉시 빠져나가는 명령어
            }

            GlobalValue.g_Unique_ID = a_IdStr;  //유저의 고유번호

            //"{\"" 문자열 인덱스 앞부분을 제거
            string a_GetStr = sz.Substring(sz.IndexOf("{\""));
            //a_GetStr = a_GetStr.Replace("\nLogin_Success!!", "");
            // "\nLogin_Success!!" 이후의 뒷부분을 제거
            int endIndex = a_GetStr.IndexOf("\nLogin_Success!!");
            a_GetStr = a_GetStr.Substring(0, endIndex);
            //Debug.Log(a_GetStr); //JSON 형식만 추출해서 가져옴

            UserInfoRespon a_Respon = JsonUtility.FromJson<UserInfoRespon>(a_GetStr);

            GlobalValue.g_NickName = a_Respon.nick_name;
            GlobalValue.g_BestScore = a_Respon.best_score;
            GlobalValue.g_UserGold = a_Respon.game_gold;

            //--- 층정보 로딩해 오기...
            if(string.IsNullOrEmpty(a_Respon.floor_info) == false)
            {
                FloorInfo a_FlooInfo = JsonUtility.FromJson<FloorInfo>(a_Respon.floor_info);
                if(a_FlooInfo != null)
                {
                    GlobalValue.g_CurFloorNum = a_FlooInfo.CurFloor;
                    GlobalValue.g_BestFloor   = a_FlooInfo.BestFloor;
                }
            }
            //--- 층정보 로딩해 오기...

            //--- ItemList 로딩해 오기...
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
            //--- ItemList 로딩해 오기...

            //로그인 성공시에...
            if (m_RefTitleMgr != null)
                m_RefTitleMgr.LoginOkGoLobby(a_IdStr);

        }//if(a_www.error == null) //에러가 발생하지 않은 경우
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

        //타임아웃 설정 
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        a_www.SendWebRequest();    //서버에 요청

        //--- 응답이 오거나 타임아웃이 발생할 때까지 대기
        while(!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;  //다음 프레임까지 대기
        }
        //--- 응답이 오거나 타임아웃이 발생할 때까지 대기

        //--- 타임아웃 처리
        if(isTimeOut == true)
        {
            a_www.Abort();  //요청을 중단
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }
        //--- 타임아웃 처리

        if (a_www.error == null)  //에러가 없을 때
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_www.downloadHandler.data);

            if (m_RefTitleMgr != null)
            {
                if (sz.Contains("Create Success.") == true)
                {
                    m_RefTitleMgr.IdInputField.text = a_IdStr;
                    m_RefTitleMgr.PwInputField.text = a_PwStr;
                    m_RefTitleMgr.MessageOnOff("가입 성공");
                }
                else if (sz.Contains("ID does exist.") == true)
                    m_RefTitleMgr.MessageOnOff("중복된 ID가 존재합니다.");
                else if (sz.Contains("Nickname does exist.") == true)
                    m_RefTitleMgr.MessageOnOff("중복된 별명이 존재합니다.");
                else
                    m_RefTitleMgr.MessageOnOff(sz);
            }//if (m_RefTitleMgr != null)
        }
        else
        {
            if (m_RefTitleMgr != null)
                m_RefTitleMgr.MessageOnOff("가입 실패 : " + a_www.error);
        }

        a_www.Dispose();
        NetworkMgr.Inst.m_NetWaitTimer = 0.0f;

    }//IEnumerator CreateActCo(string a_IdStr, string a_PwStr, string a_NickStr)
}
