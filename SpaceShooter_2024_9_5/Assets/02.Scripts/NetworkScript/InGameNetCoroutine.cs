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
        // as 연산자는 주어진 객체가 지정한 타입으로 캐스팅이 가능한지 확인하고,
        // 가능하면 해당 타입으로 변환하며, 불가능하면 null을 반환합니다.
        m_RefGameMgr = a_CurMgr as GameMgr;
    }

    public IEnumerator UpdateScoreCo()
    {
        if(GlobalValue.g_Unique_ID == "")  //정상적으로 로그인 되어 있지 않다면...
            yield break;    //코루틴 함수를 빠져 나가기...

        WWWForm form = new WWWForm();   
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_score", GlobalValue.g_BestScore);

        //타임아웃 설정 (초 단위로 설정, 예: 3초)
        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        UnityWebRequest a_www = UnityWebRequest.Post(BestScoreUrl, form);

        a_www.SendWebRequest(); //서버에 요청

        //응답이 오거나 타임아웃이 발생할 때까지 대기
        while( !a_www.isDone && !isTimeOut )
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;  // 다음 프레임까지 대기
        }//while( !a_www.isDone && !isTimeOut )

        //타임아웃 처리
        if(isTimeOut == true)
        {
            a_www.Abort(); //요청을 중단
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        if(a_www.error == null)  //에러가 나지 않았을 때 동작
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

        a_www.SendWebRequest();     //서버에 요청

        //응답이 오거나 타임아웃이 발생할 때까지 대기
        while (!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;  // 다음 프레임까지 대기
        }//while( !a_www.isDone && !isTimeOut )

        //타임아웃 처리
        if (isTimeOut == true)
        {
            a_www.Abort(); //요청을 중단
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        if (a_www.error == null)  //에러가 나지 않았을 때 동작
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

        //--- JSON 만들기...
        FloorInfo a_FInfo = new FloorInfo();
        a_FInfo.CurFloor  = GlobalValue.g_CurFloorNum;
        a_FInfo.BestFloor = GlobalValue.g_BestFloor;
        string a_StrJson = JsonUtility.ToJson(a_FInfo);
        //--- JSON 만들기...

        WWWForm form = new WWWForm();
        form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        form.AddField("Input_floor", a_StrJson, System.Text.Encoding.UTF8);

        bool isTimeOut = false;
        float startTime = Time.unscaledTime;

        NetworkMgr.Inst.m_NetWaitTimer = NetworkMgr.m_Timeout;

        UnityWebRequest a_www = UnityWebRequest.Post(UpdateFloorUrl, form);

        a_www.SendWebRequest();     //서버에 요청

        //응답이 오거나 타임아웃이 발생할 때까지 대기
        while (!a_www.isDone && !isTimeOut)
        {
            if (Time.unscaledTime - startTime > NetworkMgr.m_Timeout)
                isTimeOut = true;

            yield return null;  // 다음 프레임까지 대기
        }//while( !a_www.isDone && !isTimeOut )

        //타임아웃 처리
        if (isTimeOut == true)
        {
            a_www.Abort(); //요청을 중단
            NetworkMgr.Inst.m_NetWaitTimer = 0.0f;
            yield break;
        }

        if (a_www.error == null)  //에러가 나지 않았을 때 동작
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
