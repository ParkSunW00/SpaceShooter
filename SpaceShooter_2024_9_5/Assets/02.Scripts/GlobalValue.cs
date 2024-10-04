using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Skill_0 = 0,       //30% 힐링
    Skill_1,           //수류탄
    Skill_2,           //보호막
    SkCount
}

public class GlobalValue 
{
    public static string g_Unique_ID = "";  //유저의 고유번호

    public static string g_NickName = "";   //유저의 별명
    public static int g_BestScore = 0;      //게임 점수
    public static int g_UserGold  = 0;      //게임 머니
    public static int g_Exp = 0;            //경험치 Experience
    public static int g_Level = 0;          //레벨

    public static int[] g_SkillCount = new int[3];  //아이템 보유수

    public static int g_BestFloor = 1;      //최종 도달(클리어) 건물 층수
    public static int g_CurFloorNum = 1;    //현재 건물 층수

    public static void LoadGameData()
    {
        // PlayerPrefs.DeleteAll();
        //PlayerPrefs.SetInt("UserGold", 9999);
        //PlayerPrefs.SetInt("BestFloorNum", 20);

        //g_NickName  = PlayerPrefs.GetString("NickName", "SBS영웅");
        //g_BestScore = PlayerPrefs.GetInt("BestScore", 0);
        //g_UserGold  = PlayerPrefs.GetInt("UserGold", 0);

        //string a_MkKey = "";
        //for(int i = 0; i < g_SkillCount.Length; i++)
        //{
        //    a_MkKey = "SkItem_" + i.ToString();
        //    //PlayerPrefs.SetInt(a_MkKey, 5);
        //    g_SkillCount[i] = PlayerPrefs.GetInt(a_MkKey, 1);
        //}

        //PlayerPrefs.SetInt("BestFloorNum", 100);
        //PlayerPrefs.SetInt("CurFloorNum", 100);

        //g_BestFloor = PlayerPrefs.GetInt("BestFloorNum", 1);
        //g_CurFloorNum = PlayerPrefs.GetInt("CurFloorNum", 1);

    }//public static void LoadGameData()

    public static void ClearGameData()
    {
        g_Unique_ID = ""; //유저 고유번호
        g_NickName = "";  //유저의 별명
        g_BestScore = 0;  //게임 점수
        g_UserGold  = 0;  //게임 머니
        g_Exp = 0;        //경험치
        g_Level = 0;      //레벨

        g_BestFloor = 1;    //최종 도달 층
        g_CurFloorNum = 1;  //현재층

        for(int i = 0; i < g_SkillCount.Length; i++) 
        {
            g_SkillCount[i] = 0;
        }
    }//public static void ClearGameData()

} //public class GlobalValue 
