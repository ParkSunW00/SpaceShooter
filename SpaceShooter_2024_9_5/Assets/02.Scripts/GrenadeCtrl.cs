using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeCtrl : MonoBehaviour
{
    //폭발 효과 파티클 연결 함수
    public GameObject expEffect;

    //폭발 지연 타이머
    float timer = 2.0f;

    //무작위로 선택할 텍스쳐 배열
    public Texture[] textures;

    //수류탄 날아가는 속도
    float speed = 450.0f;
    Vector3 m_ForwardDir = Vector3.zero;

    bool isRot = true;

    // Start is called before the first frame update
    void Start()
    {
        int idx = Random.Range(0, textures.Length);
        GetComponentInChildren<MeshRenderer>().material.mainTexture = textures[idx];

        //--- 날아가는 방향 재조정
        transform.forward = m_ForwardDir;
        transform.eulerAngles = 
                new Vector3(20.0f, transform.eulerAngles.y, transform.eulerAngles.z);
        GetComponent<Rigidbody>().AddForce(m_ForwardDir * speed);
        //--- 날아가는 방향 재조정
    }

    // Update is called once per frame
    void Update()
    {
        if(0.0f < timer)
        {
            timer -= Time.deltaTime;
            if(timer <= 0.0f)
            {
                ExpGrenade();
            }
        }

        if(isRot == true)
        {
            transform.Rotate(new Vector3(Time.deltaTime * 190.0f, 0.0f, 0.0f), Space.Self);
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        isRot = false;
    }

    void ExpGrenade()
    {
        //폭발 효과 파티클 생성
        GameObject explotion = Instantiate(expEffect, transform.position, Quaternion.identity);
        Destroy(explotion, explotion.GetComponentInChildren<ParticleSystem>().main.duration + 2.0f);

        //지정한 원점을 중심으로 10.0f 반경 내에 들어와 있는 Collider 객체 추출
        Collider[] colls = Physics.OverlapSphere(transform.position, 10.0f);

        //추출한 Collider 객체에 폭발력 전달
        MonsterCtrl a_MonCtrl = null;
        foreach(Collider coll in  colls)
        {
            a_MonCtrl = coll.GetComponent<MonsterCtrl>();
            if (a_MonCtrl == null)
                continue;

            a_MonCtrl.TakeDamage(150);
        }

        //즉시 제거
        Destroy(gameObject);
    }

    public void SetForwardDir(Vector3 a_Dir)
    {
        m_ForwardDir = new Vector3(a_Dir.x, a_Dir.y + 0.65f, a_Dir.z);
    }

}
