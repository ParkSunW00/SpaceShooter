using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


//클래스에 System.Serializable 이라는 어트리뷰트(Attribute)를 명시해야
//Inspector 뷰에 노출됨
[System.Serializable]
public class Anim
{
    public AnimationClip idle;
    public AnimationClip runForward;
    public AnimationClip runBackward;
    public AnimationClip runRight;
    public AnimationClip runLeft;
}

public class PlayerCtrl : MonoBehaviour
{
    private float h = 0.0f;
    private float v = 0.0f;

    //이동 속도 변수
    public float moveSpeed = 10.0f;
    float m_GReturnTime = 0.69f; //m_JumpPower를 다 깍아먹고 (-m_VelocotyY)에 도달해야 하는데 걸리는 시간
    float m_GravitySpeed = 36.2f;
    float m_VelocityY = -12.0f;  //중력(밑으로 끌어 내리는 힘), 중력 가속도의 최대치 : -12.0f;
    float m_JumpPower = 13.0f;   //점프시 뛰어 오르는 힘
    //bool m_CanDoubleJump = false;

    //회전 속도 변수
    public float rotSpeed = 100.0f;

    //인스펙터뷰에 표시할 애니메이션 클래스 변수
    public Anim anim;

    //아래에 있는 3D 모델의 Animation 컴포넌트에 접근하기 위한 변수
    public Animation _animation;

    //Player의 생명 변수
    public int hp = 100;
    //Player의 생명 초깃값
    private int initHp;
    //Player의 Health bar 이미지
    public Image imgHpbar;

    CharacterController m_ChrCtrl;  //현재 캐릭터가 가지고 있는 캐릭터 컨트롤러 참조 변수

    FireCtrl m_FireCtrl = null;
    public GameObject bloodEffect;  //혈흔 효과 프리팹

    //--- 쉴드 스킬
    float m_SdDurtion = 20.0f;
    float m_SdOnTime = 0.0f;
    public GameObject ShieldObj = null;
    //--- 쉴드 스킬

    //--- Ending Scene 로딩 즉시 비행선과 충똘되는 현상을 방지하기 위한 타이머
    float m_CkTimer = 0.3f;
    //--- Ending Scene 로딩 즉시 비행선과 충똘되는 현상을 방지하기 위한 타이머

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        moveSpeed = 7.0f;   //이동 속도 초기화
        m_GravitySpeed = (m_JumpPower + (-m_VelocityY)) / m_GReturnTime;
        //m_GReturnTime초 만에 m_JumpPower를 다 깍아먹고 (-m_VelocityY)에 도달해야 하는 속도

        //생명 초깃값 설정
        initHp = hp;

        //자신의 하위에 있는 Animation 컴포넌트를 찾아와 변수에 할당
        _animation = GetComponentInChildren<Animation>();

        //Animation 컴포넌트의 애니메이션 클립을 지정하고 실행
        _animation.clip = anim.idle;
        _animation.Play();

        m_ChrCtrl = GetComponent<CharacterController>();
        m_FireCtrl = GetComponent<FireCtrl>();

    }//void Start()

    // Update is called once per frame
    void Update()
    {
        if(SceneManager.GetActiveScene().name == "scLevel02")
        {
            if(0.0f < m_CkTimer)
            {
                transform.position = new Vector3(11.0f, 0.07f, 12.7f);
                transform.eulerAngles = new Vector3(0.0f, -127.7f, 0.0f);

                m_CkTimer -= Time.deltaTime;
                return;
            }
        }//if(SceneManager.GetActiveScene().name == "scLevel02")

        if (GameMgr.Inst.m_GameState == GameState.GameEnd)
            return;

        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        //전후좌우 이동 방향 벡터 계산
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        if (1.0f < moveDir.magnitude)
            moveDir.Normalize();

        ////Translate(이동 방향 * Time.deltaTime * 변위값 * 속도, 기준좌표)
        //transform.Translate(moveDir * Time.deltaTime * moveSpeed, Space.Self);

        if(m_ChrCtrl != null)
        {
            //벡터를 로컬 좌표계 기준에서 월드 좌표계 기준으로 변환한다.
            moveDir = transform.TransformDirection(moveDir);
            moveDir = moveDir * moveSpeed;

            if(m_ChrCtrl.isGrounded == true) //발바닥이 바닥에 닿았을 때
            {
                if(Input.GetKeyDown(KeyCode.Space) == true)
                {
                    m_VelocityY = m_JumpPower;
                }
            }

            if (-12.0f < m_VelocityY)
                m_VelocityY -= m_GravitySpeed * Time.deltaTime;

            moveDir.y = m_VelocityY;

            m_ChrCtrl.Move(moveDir * Time.deltaTime);
        }//if(m_ChrCtrl != null)

        //--- 카메라 컨트롤 모드에 따른 분기 처리
        bool IsMsRot = false;
        float a_AddRotSpeed = 3.0f;
        if(FollowCam.m_CCMMode == CamCtrlMode.CCM_Default)
        {
            if (FollowCam.m_CCMDelay <= 0.0f)
                IsMsRot = true;

            a_AddRotSpeed = 2.1f;
        }
        else
        {
            if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
            if(GameMgr.IsPointerOverUIObject() == false)
            {
                IsMsRot = true;
            }
        }
        //--- 카메라 컨트롤 모드에 따른 분기 처리

        //if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        if(IsMsRot == true)
        {
            //Vector3.up 축을 기준으로 rotSpeed 만큼 의 속도로 회전
            transform.Rotate(Vector3.up * Time.deltaTime * rotSpeed * Input.GetAxis("Mouse X") * a_AddRotSpeed);
        }//if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)

        //키보드 입력값을 기준으로 동작할 애니메이션 수행
        if (v >= 0.01f)
        {
            //전진 애니메이션
            _animation.CrossFade(anim.runForward.name, 0.3f);
        }
        else if (v <= -0.01f)
        {
            //후진 애니메이션
            _animation.CrossFade(anim.runBackward.name, 0.3f);
        }
        else if(h >= 0.01f)
        {
            //오른쪽 이동 애니메이션
            _animation.CrossFade(anim.runRight.name, 0.3f);
        }
        else if(h <= -0.01f)
        {
            //왼쪽 이동 애니메이션
            _animation.CrossFade(anim.runLeft.name, 0.3f);
        }
        else
        {
            //정지시 idle 애니메이션
            _animation.CrossFade(anim.idle.name, 0.3f);
        }

        SkillUpdate();

    }//void Update()

    //충돌한 Collider의 IsTrigger 옵션이 체크됐을 때 발생
    void OnTriggerEnter(Collider coll)
    {
        //충돌한 Collider가 몬스터의 PUNCH이면 Player의 HP 차감
        if(coll.gameObject.tag == "PUNCH")
        {
            if (0.0f < m_SdOnTime)  //쉴드 발동 중이면...
                return;

            if (hp <= 0.0f)  //이미 사망한 상태면...
                return;

            hp -= 10;

            //Image UI 항목의 fillAmount 속성을 조절해 생명 게이지 값 조정
            imgHpbar.fillAmount = (float)hp / (float)initHp;

            //Debug.Log("Player HP = " + hp.ToString());

            //Player의 생명이 0이하이면 사망 처리
            if(hp <= 0)
            {
                PlayerDie();
            }
        }
        else if(coll.gameObject.name.Contains("CoinPrefab") == true)
        {
            int a_CacGold = 10;

            GameMgr.Inst.AddGold(a_CacGold);

            Destroy(coll.gameObject);
        }
        else if(coll.gameObject.name.Contains("Gate_Exit_1") == true ||
                coll.gameObject.name.Contains("Gate_Exit_2") == true)
        {
            GlobalValue.g_CurFloorNum++;
            //PlayerPrefs.SetInt("CurFloorNum", GlobalValue.g_CurFloorNum);

            if(GlobalValue.g_BestFloor < GlobalValue.g_CurFloorNum)
            {
                GlobalValue.g_BestFloor = GlobalValue.g_CurFloorNum;
                //PlayerPrefs.SetInt("BestFloorNum", GlobalValue.g_BestFloor);
            }

            if(GlobalValue.g_CurFloorNum < 100)
            {
                SceneManager.LoadScene("scLevel01");
                SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.LoadScene("scLevel02");
                SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
            }
        }//else if(coll.gameObject.name.Contains("Gate_Exit_1") == true ||
        else if(coll.gameObject.name.Contains("DiamondPrefab") == true)
        {
            GameMgr.Inst.ShowDoor();  //다이아몬드를 회수 했으면 문이 열린다.

            Destroy(coll.gameObject);
        }//else if(coll.gameObject.name.Contains("DiamondPrefab") == true)
        else if(coll.gameObject.name.Contains("RTS15_desert") == true)
        {
            if (m_CkTimer <= 0.0f)
                PlayerDie();         //게임엔딩
        }


    }//void OnTriggerEnter(Collider coll)

    void OnTriggerStay(Collider coll)
    {
        if(coll.gameObject.name.Contains("Gate_In_1") == true)
        {
            if (Input.GetKey(KeyCode.LeftShift) == false)
                return;

            GlobalValue.g_CurFloorNum--;
            if (GlobalValue.g_CurFloorNum < 1)
                GlobalValue.g_CurFloorNum = 1;

            //PlayerPrefs.SetInt("CurFloorNum", GlobalValue.g_CurFloorNum);

            SceneManager.LoadScene("scLevel01");
            SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
        }
    }//void OnTriggerStay(Collider coll)

    void OnCollisionEnter(Collision coll)
    {
        if(coll.gameObject.tag == "E_BULLET")
        {
            //-- 혈흔 효과 생성
            GameObject blood = (GameObject)Instantiate(bloodEffect,
                                coll.transform.position, Quaternion.identity);

            blood.GetComponent<ParticleSystem>().Play();
            Destroy(blood, 3.0f);
            //-- 혈흔 효과 생성

            Destroy(coll.gameObject);   //E_BULLET 삭제

            if (hp <= 0.0f)
                return;

            hp -= 10;

            if (imgHpbar == null)
                imgHpbar = GameObject.Find("Hp_Image").GetComponent<Image>();

            if (imgHpbar != null)
                imgHpbar.fillAmount = (float)hp / (float)initHp;

            if(hp <= 0)
            {
                PlayerDie();
            }

        }//if(coll.gameObject.tag == "E_BULLET")
    }

    //Player의 사망 처리 루틴
    void PlayerDie()
    {
        //Debug.Log("Player Die !!");

        //MONSTER라는 Tag를 가진 모든 게임오브젝트를 찾아옴
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
        foreach(GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
        }

        _animation.Stop(); //애니메이션 컴포넌트의 애니메이션 중지 함수

        GameMgr.Inst.m_GameState = GameState.GameEnd;
        ////GameMgr의 싱글턴 인스턴스를 접근해 isGameOver 변숫값을 변경
        //GameMgr.Inst.isGameOver = true;
        Time.timeScale = 0.0f;  //일시정지
        GameMgr.Inst.GameOverMethod();

    }//void PlayerDie()

    void SkillUpdate()
    {
        //--- 쉴드 상태 업데이트
        if(0.0f < m_SdOnTime)
        {
            m_SdOnTime -= Time.deltaTime;
            if(ShieldObj != null && ShieldObj.activeSelf == false)
                ShieldObj.SetActive(true);
        }
        else
        {
            if (ShieldObj != null && ShieldObj.activeSelf == true)
                ShieldObj.SetActive(false);
        }
        //--- 쉴드 상태 업데이트
    }

    public void UseSkill_Item(SkillType a_SkType)
    {
        if (GameMgr.Inst.m_GameState == GameState.GameEnd)
            return;

        if(a_SkType == SkillType.Skill_0)   //30% 힐링 아이템 스킬
        {
            //머리위 텍스트 띄우기
            Vector3 a_CurPos = transform.position;
            a_CurPos.y += 2.21f;
            GameMgr.Inst.SpawnDamageText((int)(initHp * 0.3f), a_CurPos, Color.white, true);

            hp += (int)(initHp * 0.3f);

            if(initHp < hp)
                hp = initHp;    

            if(imgHpbar != null)
               imgHpbar.fillAmount = hp / (float)initHp;
        }
        else if(a_SkType == SkillType.Skill_1) //수류탄
        {
            if (m_FireCtrl != null)
                m_FireCtrl.FireGrenade();
        }
        else if(a_SkType == SkillType.Skill_2)  //보호막
        {
            if (0.0f < m_SdOnTime)
                return;

            m_SdOnTime = m_SdDurtion;

            //쿨타임 발동
            GameMgr.Inst.SkillTimeMethod(m_SdOnTime, m_SdDurtion);
        }

        int a_SkIdx = (int)a_SkType;
        GlobalValue.g_SkillCount[a_SkIdx]--;
        string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);

    }//public void UseSkill_Item(SkillType a_SkType)
}
