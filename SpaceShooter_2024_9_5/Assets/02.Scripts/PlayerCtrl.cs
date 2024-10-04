using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


//Ŭ������ System.Serializable �̶�� ��Ʈ����Ʈ(Attribute)�� ����ؾ�
//Inspector �信 �����
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

    //�̵� �ӵ� ����
    public float moveSpeed = 10.0f;
    float m_GReturnTime = 0.69f; //m_JumpPower�� �� ��Ƹ԰� (-m_VelocotyY)�� �����ؾ� �ϴµ� �ɸ��� �ð�
    float m_GravitySpeed = 36.2f;
    float m_VelocityY = -12.0f;  //�߷�(������ ���� ������ ��), �߷� ���ӵ��� �ִ�ġ : -12.0f;
    float m_JumpPower = 13.0f;   //������ �پ� ������ ��
    //bool m_CanDoubleJump = false;

    //ȸ�� �ӵ� ����
    public float rotSpeed = 100.0f;

    //�ν����ͺ信 ǥ���� �ִϸ��̼� Ŭ���� ����
    public Anim anim;

    //�Ʒ��� �ִ� 3D ���� Animation ������Ʈ�� �����ϱ� ���� ����
    public Animation _animation;

    //Player�� ���� ����
    public int hp = 100;
    //Player�� ���� �ʱ갪
    private int initHp;
    //Player�� Health bar �̹���
    public Image imgHpbar;

    CharacterController m_ChrCtrl;  //���� ĳ���Ͱ� ������ �ִ� ĳ���� ��Ʈ�ѷ� ���� ����

    FireCtrl m_FireCtrl = null;
    public GameObject bloodEffect;  //���� ȿ�� ������

    //--- ���� ��ų
    float m_SdDurtion = 20.0f;
    float m_SdOnTime = 0.0f;
    public GameObject ShieldObj = null;
    //--- ���� ��ų

    //--- Ending Scene �ε� ��� ���༱�� ��ʵǴ� ������ �����ϱ� ���� Ÿ�̸�
    float m_CkTimer = 0.3f;
    //--- Ending Scene �ε� ��� ���༱�� ��ʵǴ� ������ �����ϱ� ���� Ÿ�̸�

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        moveSpeed = 7.0f;   //�̵� �ӵ� �ʱ�ȭ
        m_GravitySpeed = (m_JumpPower + (-m_VelocityY)) / m_GReturnTime;
        //m_GReturnTime�� ���� m_JumpPower�� �� ��Ƹ԰� (-m_VelocityY)�� �����ؾ� �ϴ� �ӵ�

        //���� �ʱ갪 ����
        initHp = hp;

        //�ڽ��� ������ �ִ� Animation ������Ʈ�� ã�ƿ� ������ �Ҵ�
        _animation = GetComponentInChildren<Animation>();

        //Animation ������Ʈ�� �ִϸ��̼� Ŭ���� �����ϰ� ����
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

        //�����¿� �̵� ���� ���� ���
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        if (1.0f < moveDir.magnitude)
            moveDir.Normalize();

        ////Translate(�̵� ���� * Time.deltaTime * ������ * �ӵ�, ������ǥ)
        //transform.Translate(moveDir * Time.deltaTime * moveSpeed, Space.Self);

        if(m_ChrCtrl != null)
        {
            //���͸� ���� ��ǥ�� ���ؿ��� ���� ��ǥ�� �������� ��ȯ�Ѵ�.
            moveDir = transform.TransformDirection(moveDir);
            moveDir = moveDir * moveSpeed;

            if(m_ChrCtrl.isGrounded == true) //�߹ٴ��� �ٴڿ� ����� ��
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

        //--- ī�޶� ��Ʈ�� ��忡 ���� �б� ó��
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
        //--- ī�޶� ��Ʈ�� ��忡 ���� �б� ó��

        //if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)
        if(IsMsRot == true)
        {
            //Vector3.up ���� �������� rotSpeed ��ŭ �� �ӵ��� ȸ��
            transform.Rotate(Vector3.up * Time.deltaTime * rotSpeed * Input.GetAxis("Mouse X") * a_AddRotSpeed);
        }//if (Input.GetMouseButton(0) == true || Input.GetMouseButton(1) == true)

        //Ű���� �Է°��� �������� ������ �ִϸ��̼� ����
        if (v >= 0.01f)
        {
            //���� �ִϸ��̼�
            _animation.CrossFade(anim.runForward.name, 0.3f);
        }
        else if (v <= -0.01f)
        {
            //���� �ִϸ��̼�
            _animation.CrossFade(anim.runBackward.name, 0.3f);
        }
        else if(h >= 0.01f)
        {
            //������ �̵� �ִϸ��̼�
            _animation.CrossFade(anim.runRight.name, 0.3f);
        }
        else if(h <= -0.01f)
        {
            //���� �̵� �ִϸ��̼�
            _animation.CrossFade(anim.runLeft.name, 0.3f);
        }
        else
        {
            //������ idle �ִϸ��̼�
            _animation.CrossFade(anim.idle.name, 0.3f);
        }

        SkillUpdate();

    }//void Update()

    //�浹�� Collider�� IsTrigger �ɼ��� üũ���� �� �߻�
    void OnTriggerEnter(Collider coll)
    {
        //�浹�� Collider�� ������ PUNCH�̸� Player�� HP ����
        if(coll.gameObject.tag == "PUNCH")
        {
            if (0.0f < m_SdOnTime)  //���� �ߵ� ���̸�...
                return;

            if (hp <= 0.0f)  //�̹� ����� ���¸�...
                return;

            hp -= 10;

            //Image UI �׸��� fillAmount �Ӽ��� ������ ���� ������ �� ����
            imgHpbar.fillAmount = (float)hp / (float)initHp;

            //Debug.Log("Player HP = " + hp.ToString());

            //Player�� ������ 0�����̸� ��� ó��
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
            GameMgr.Inst.ShowDoor();  //���̾Ƹ�带 ȸ�� ������ ���� ������.

            Destroy(coll.gameObject);
        }//else if(coll.gameObject.name.Contains("DiamondPrefab") == true)
        else if(coll.gameObject.name.Contains("RTS15_desert") == true)
        {
            if (m_CkTimer <= 0.0f)
                PlayerDie();         //���ӿ���
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
            //-- ���� ȿ�� ����
            GameObject blood = (GameObject)Instantiate(bloodEffect,
                                coll.transform.position, Quaternion.identity);

            blood.GetComponent<ParticleSystem>().Play();
            Destroy(blood, 3.0f);
            //-- ���� ȿ�� ����

            Destroy(coll.gameObject);   //E_BULLET ����

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

    //Player�� ��� ó�� ��ƾ
    void PlayerDie()
    {
        //Debug.Log("Player Die !!");

        //MONSTER��� Tag�� ���� ��� ���ӿ�����Ʈ�� ã�ƿ�
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //��� ������ OnPlayerDie �Լ��� ���������� ȣ��
        foreach(GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
        }

        _animation.Stop(); //�ִϸ��̼� ������Ʈ�� �ִϸ��̼� ���� �Լ�

        GameMgr.Inst.m_GameState = GameState.GameEnd;
        ////GameMgr�� �̱��� �ν��Ͻ��� ������ isGameOver �������� ����
        //GameMgr.Inst.isGameOver = true;
        Time.timeScale = 0.0f;  //�Ͻ�����
        GameMgr.Inst.GameOverMethod();

    }//void PlayerDie()

    void SkillUpdate()
    {
        //--- ���� ���� ������Ʈ
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
        //--- ���� ���� ������Ʈ
    }

    public void UseSkill_Item(SkillType a_SkType)
    {
        if (GameMgr.Inst.m_GameState == GameState.GameEnd)
            return;

        if(a_SkType == SkillType.Skill_0)   //30% ���� ������ ��ų
        {
            //�Ӹ��� �ؽ�Ʈ ����
            Vector3 a_CurPos = transform.position;
            a_CurPos.y += 2.21f;
            GameMgr.Inst.SpawnDamageText((int)(initHp * 0.3f), a_CurPos, Color.white, true);

            hp += (int)(initHp * 0.3f);

            if(initHp < hp)
                hp = initHp;    

            if(imgHpbar != null)
               imgHpbar.fillAmount = hp / (float)initHp;
        }
        else if(a_SkType == SkillType.Skill_1) //����ź
        {
            if (m_FireCtrl != null)
                m_FireCtrl.FireGrenade();
        }
        else if(a_SkType == SkillType.Skill_2)  //��ȣ��
        {
            if (0.0f < m_SdOnTime)
                return;

            m_SdOnTime = m_SdDurtion;

            //��Ÿ�� �ߵ�
            GameMgr.Inst.SkillTimeMethod(m_SdOnTime, m_SdDurtion);
        }

        int a_SkIdx = (int)a_SkType;
        GlobalValue.g_SkillCount[a_SkIdx]--;
        string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);

    }//public void UseSkill_Item(SkillType a_SkType)
}
