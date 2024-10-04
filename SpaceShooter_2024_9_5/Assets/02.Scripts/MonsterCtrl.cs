using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class MonsterCtrl : MonoBehaviour
{
    //몬스터의 상태 정보가 있는 Enumerable 변수 선언
    public enum MonsterState { idle, trace, attack, die };

    //몬스터의 현재 상태 정보를 저장할 Enum 변수
    public MonsterState monsterState = MonsterState.idle;

    //속도 향상을 위해 각종 컴포넌트를 변수에 할당
    private Transform monsterTr;
    private Transform playerTr;
    //private NavMeshAgent nvAgent;
    private Animator animator; 

    //추적 사정거리
    public float traceDist = 10.0f;
    //공격 사정거리
    public float attackDist = 2.0f;

    //몬스터의 사망 여부
    private bool isDie = false;

    //혈흔 효과 프리팹
    public GameObject bloodEffect;
    //혈흔 데칼 효과 프리팹
    public GameObject bloodDecal;

    //몬스터 생명 변수
    private int hp = 100;
    Rigidbody m_Rigid = null;

    //--- 총알 발사 관련 변수
    public GameObject bullet;       //총알 프리팹
    float m_BLTime = 0.0f;
    LayerMask m_LaserMask = -1;
    //--- 총알 발사 관련 변수

    void Awake()
    {
        traceDist = 10.0f;
        attackDist = 1.5f;

        //몬스터의 Transform 할당
        monsterTr = this.gameObject.GetComponent<Transform>();
        //추적 대상인 Player의 Transform 할당
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        ////NavMeshAgent 컴포넌트 할당
        //nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        //Animator 컴포넌트 할당
        animator = this.gameObject.GetComponent<Animator>();

        m_Rigid = GetComponent<Rigidbody>();
    }

    ////이벤트 발생시 수행할 함수 연결
    //void OnEnable()
    //{
    //    //일정한 간격으로 몬스터의 행동 상태를 체크하는 코루틴 함수 실행
    //    StartCoroutine(this.CheckMonsterState());

    //    //몬스터의 상태에 따라 동작하는 루틴을 실행하는 코루틴 함수 실행
    //    StartCoroutine(this.MonsterAction());
    //}

    // Start is called before the first frame update
    void Start()
    {
        m_LaserMask = 1 << LayerMask.NameToLayer("Default");
        //"Default" 레이어만 레이 캐스트 체크 하겠다는 설정
    }

    // Update is called once per frame
    void FixedUpdate()   //void Update()
    {
        if (GameMgr.Inst.m_GameState == GameState.GameEnd)
            return;

        if (playerTr.gameObject.activeSelf == false)
            playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();

        CheckMonStateUpdate();
        MonActionUpdate();
    }

#region -- 몬스터 AI

    //일정한 간격으로 몬스터의 행동 상태를 체크하고 monsterState 값 변경
    float m_AI_Delay = 0.0f;

    void CheckMonStateUpdate()
    {
        if (isDie == true)
            return;

        //0.1초 주기로만 체크하기 위한 딜레이 계산 부분
        m_AI_Delay -= Time.deltaTime;
        if (0.0f < m_AI_Delay)
            return;

        m_AI_Delay = 0.1f;
        //0.1초 주기로만 체크하기 위한 딜레이 계산 부분

        //몬스터와 플레이어 사이의 거리 측정
        float dist = Vector3.Distance(playerTr.position, monsterTr.position);
        float Ydist = Mathf.Abs(playerTr.position.y - monsterTr.position.y);

        if(dist <= attackDist) //공격거리 범위 이내로 들어왔는지 확인
        {
            monsterState = MonsterState.attack;
        }
        else if(dist <= traceDist && Ydist < 5.0f) //추적거리 범위 이내로 들어왔는지 확인
        {
            monsterState = MonsterState.trace;   //몬스터의 상태를 추적으로 설정
        }
        else
        {
            monsterState = MonsterState.idle;   //몬스터의 상태를 idle 모드로 설정
        }

    }//void CheckMonStateUpdate()

    //몬스터의 상태값에 따라 적절한 동작을 수행하는 함수
    void MonActionUpdate()
    {
        if (isDie == true)
            return;

        switch(monsterState)
        {
            //idle 상태
            case MonsterState.idle:
                //Animator의 IsTrace 변수를 false 로 설정
                animator.SetBool("IsTrace", false);
                break;

            //추적상태
            case MonsterState.trace:
                {
                    //추적 이동 구현
                    float a_MoveVelocity = 2.0f; //평면 초당 이동 속도...
                    Vector3 a_MoveDir = playerTr.position - transform.position;
                    a_MoveDir.y = 0.0f;

                    if(0.0f < a_MoveDir.magnitude)
                    {
                        Vector3 a_StepVec = a_MoveDir.normalized * a_MoveVelocity * Time.deltaTime;
                        transform.Translate(a_StepVec, Space.World);

                        //--- 이동 방향을 바라보도록 회전 처리
                        float a_RotSpeed = 7.0f;    //초당 회전 속도
                        Quaternion a_TargetRot = Quaternion.LookRotation(a_MoveDir.normalized);
                        transform.rotation = Quaternion.Slerp(transform.rotation,
                                                        a_TargetRot, Time.deltaTime * a_RotSpeed);
                        //--- 이동 방향을 바라보도록 회전 처리
                    }//if(0.0f < a_MoveDir.magnitude)

                    //Animator의 IsAttack 변수를 false로 설정
                    animator.SetBool("IsAttack", false);

                    //Animator의 IsTrace 변수를 true로 설정
                    animator.SetBool("IsTrace", true);
                }
                break;

            //공격 상태
            case MonsterState.attack:
                {
                    //IsAttack을 true로 설정해 attack State로 전이
                    animator.SetBool("IsAttack", true);

                    //--- 몬스터가 주인공을 공격하면서 바라보도록 처리
                    float a_RotSpeed = 6.0f;    //초당 회전 속도
                    Vector3 a_CacDir = playerTr.position - transform.position;
                    a_CacDir.y = 0.0f;
                    if(0.0f < a_CacDir.magnitude)
                    {
                        //--- AI 0.1초 간격 체크 때문에 공격 거리에서 멀어진 경우 위치 보정
                        if(attackDist < a_CacDir.magnitude)
                        {
                            float a_MoveVelocity = 2.0f;
                            Vector3 a_StepVec = a_CacDir.normalized * a_MoveVelocity * Time.deltaTime;
                            transform.Translate(a_StepVec, Space.World);
                        }
                        //--- AI 0.1초 간격 체크 때문에 공격 거리에서 멀어진 경우 위치 보정

                        Quaternion a_TargetRot = Quaternion.LookRotation(a_CacDir.normalized);
                        transform.rotation = Quaternion.Slerp(
                                    transform.rotation, a_TargetRot, Time.deltaTime * a_RotSpeed);
                    }
                    //--- 몬스터가 주인공을 공격하면서 바라보도록 처리
                }
                break;
        }//switch(monsterState)

        //--- 총알발사
        if(3 <= GlobalValue.g_CurFloorNum)  //몬스터는 3층부터 총알 발사
            FireUpdate();
        //--- 총알발사

    }//void MonActionUpdate()

 #endregion

    void FireUpdate() //주기적으로 총알 발사하는 함수
    {
        Vector3 a_PlayerPos = playerTr.position;
        a_PlayerPos.y = a_PlayerPos.y + 1.5f;
        Vector3 a_MonPos = transform.position;
        a_MonPos.y = a_MonPos.y + 1.5f;
        Vector3 a_CacDir = a_PlayerPos - a_MonPos;
        float a_RayUDLimit = 3.5f;

        m_BLTime = m_BLTime - Time.deltaTime;
        if(m_BLTime <= 0.0f)
        {
            m_BLTime = 0.0f;
        }

        //추적 거리 안쪽이면 제외
        if (a_CacDir.magnitude <= traceDist)
            return;

        //높이값 한계치 기준 위, 아래 -3.5 ~ +3.5 높이로 조준 제한을 넘어서면 제외
        if ((-a_RayUDLimit <= a_CacDir.y && a_CacDir.y <= a_RayUDLimit) == false)
            return;

        bool IsRayOk = false;
        if(Physics.Raycast(a_MonPos, a_CacDir.normalized, 
                                out RaycastHit hit, 100.0f, m_LaserMask) == true)
        {
            if(hit.collider.gameObject.tag == "Player")
            {
                IsRayOk = true;
            }
        }//if(Physics.Raycast(a_MonPos, a_CacDir.normalized,

        //몬스터에서 주인공까지의 직선 시야에 주인공이 들어오지 않으면 제외
        if (IsRayOk == false)
            return;

        //--- 몬스터가 주인공을 향해 바라 보도록 회전 처리
        Vector3 a_CacVLen = playerTr.position - transform.position;
        a_CacVLen.y = 0.0f;
        Quaternion a_TargetRot = Quaternion.LookRotation(a_CacVLen.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation,
                                    a_TargetRot, Time.deltaTime * 6.0f);
        //--- 몬스터가 주인공을 향해 바라 보도록 회전 처리

        if(m_BLTime <= 0.0f)
        {
            Vector3 a_StartPos = a_MonPos + a_CacDir.normalized * 1.5f;
            GameObject a_Bullet = Instantiate(bullet, a_StartPos, Quaternion.identity);
            a_Bullet.layer = LayerMask.NameToLayer("E_BULLET");
            a_Bullet.tag = "E_BULLET";
            a_Bullet.transform.forward = a_CacDir.normalized;

            //몬스터의 총알 발사 주기 : 2초(10층까지)에서 ~ 1초(99층)까지 변하게...
            float a_CacDf = (GlobalValue.g_CurFloorNum - 10) * 0.012f;
            if (a_CacDf < 0.0f)
                a_CacDf = 0.0f;
            if (1.0f < a_CacDf)
                a_CacDf = 1.0f;

            m_BLTime = 2.0f - a_CacDf;
        }//if(m_BLTime <= 0.0f)

    } //void FireUpdate()

    //Bullet과 충돌 체크
    void OnCollisionEnter(Collision coll)
    {
        if(coll.gameObject.tag == "BULLET")
        {
            ////--- 머리위 텍스트 띄우기
            //Vector3 a_CurPos = transform.position;
            //a_CurPos.y += 2.27f;
            //GameMgr.Inst.SpawnDamageText(-(int)(coll.gameObject.GetComponent<BulletCtrl>().damage),
            //    a_CurPos, new Color32(255, 20, 20, 255));
            ////--- 머리위 텍스트 띄우기

            //혈흔 효과 함수 호출
            CreateBloodEffect(coll.transform.position);

            //맞은 총알의 Damage를 추출해 몬스터 hp 차감
            hp -= coll.gameObject.GetComponent<BulletCtrl>().damage;
            if(hp <= 0)
            {
                MonsterDie();
            }

            //Bullet 삭제
            Destroy(coll.gameObject);
            //IsHit Trigger를 발생시키면 Any State에서 gothit로 전이됨
            animator.SetTrigger("IsHit");
        }
    }//void OnCollisionEnter(Collision coll)

    //몬스터 사망 시 처리 루틴
    void MonsterDie()
    {
        //사망한 몬스터의 태그를 Untagged로 변경
        gameObject.tag = "Untagged";

        //모든 코루틴을 정지
        StopAllCoroutines();

        isDie = true;
        monsterState = MonsterState.die;
        //nvAgent.isStopped = true;
        animator.SetTrigger("IsDie");

        m_Rigid.useGravity = false;

        //몬스터에 추가된 Collider를 비활성화
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;

        foreach(Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = false;
        }

        //GameMgr의 스코어 누적과 스코어 표시 함수 호출
        GameMgr.Inst.DispScore(1);  //50);

        //몬스터 오브젝트 풀로 환원시키는 코루틴 함수 호출
        StartCoroutine(this.PushObjectPool());

        //보상으로 아이템 드롭
        GameMgr.Inst.SpawnCoin(this.transform.position);

        //몬스터 킬 카운트 증가
        if (GameMgr.Inst.m_CurKillNum < GameMgr.Inst.m_TargetKillNum)
            GameMgr.Inst.m_CurKillNum++;

    }//void MonsterDie()

    IEnumerator PushObjectPool()
    {
        yield return new WaitForSeconds(3.0f);

        //각종 변수 초기화
        isDie = false;
        hp = 100;
        gameObject.tag = "MONSTER";
        monsterState = MonsterState.idle;

        m_Rigid.useGravity = true;

        //몬스터에 추가된 Collider을 다시 활성화
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = true;

        foreach(Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = true;
        }

        //몬스터를 비활성화
        gameObject.SetActive(false);
    }

    void CreateBloodEffect(Vector3 pos)
    {
        //혈흔 효과 생성
        GameObject blood1 = Instantiate(bloodEffect, pos, Quaternion.identity);
        blood1.GetComponent<ParticleSystem>().Play();
        Destroy(blood1, 3.0f);

        //데칼 생성 위치 - 바닥에서 조금 올린 위치 산출
        Vector3 decalPos = monsterTr.position + (Vector3.up * 0.05f);
        //데칼의 회전값을 무작위로 설정
        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));

        //데칼 프리팹 생성
        GameObject blood2 = Instantiate(bloodDecal, decalPos, decalRot);
        //데칼의 크기도 불규칙적으로 나타나게끔 스케일 조정
        float scale = Random.Range(1.5f, 3.5f);
        blood2.transform.localScale = Vector3.one * scale;

        //5초 후에 혈흔효과 프리팹을 삭제
        Destroy(blood2, 5.0f);

    }//void CreateBloodEffect(Vector3 pos)

    //플레이어가 사망했을 때 실행하는 함수
    void OnPlayerDie()
    {
        //몬스터의 상태를 체크하는 코루틴 함수를 모두 정지시킴
        StopAllCoroutines();
        ////추적을 정지하고 애니메이션을 수행
        //nvAgent.isStopped = true;
        animator.SetTrigger("IsPlayerDie");
    }

    public void TakeDamage(int a_Value)
    {
        if (hp <= 0.0f) //이렇게 하면 사망 처리는 한번만 될 것임
            return;

        //혈흔 효과 함수 호출
        CreateBloodEffect(transform.position);

        hp -= a_Value;
        if(hp <= 0)
        {
            hp = 0;
            MonsterDie();
            return;
        }

        animator.SetTrigger("IsHit");
    }

}
