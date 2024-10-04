using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
    Transform m_CameraTr = null;
    Animator m_RefAnim = null;
    Text m_RefText = null;
    float m_DamageVal = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_CameraTr = Camera.main.transform;
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    void LateUpdate()
    {
        transform.forward = m_CameraTr.forward; //빌보드
    }

    public void InitState(int cont, Vector3 a_WSpawnPos, Color a_Color)
    {
        m_RefAnim = GetComponentInChildren<Animator>();
        if(m_RefAnim != null)
        {
            AnimatorStateInfo a_AnimInfo = m_RefAnim.GetCurrentAnimatorStateInfo(0);
            float a_LifeTime = a_AnimInfo.length;   //애니메이션 플레이 시간
            Destroy(gameObject, a_LifeTime);
        }

        transform.position = a_WSpawnPos;

        m_DamageVal = cont;
        m_RefText = gameObject.GetComponentInChildren<Text>();
        if(m_RefText != null)
        {
            if (m_DamageVal <= 0)
                m_RefText.text = m_DamageVal.ToString() + " Dmg";
            else
                m_RefText.text = "+" + m_DamageVal.ToString() + " Heal";

            m_RefText.color = a_Color;
        }

    }//public void InitState(int cont, Vector3 a_WSpawnPos, Color a_Color)
}
