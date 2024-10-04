using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title_Mgr : MonoBehaviour
{
    [Header("--- LoginPanel ---")]
    public GameObject m_LoginPanelObj;
    public Button m_LoginBtn = null;
    public Button m_CreateAccOpenBtn = null;
    public InputField IdInputField;
    public InputField PwInputField;
    public Toggle SaveIdToggle;

    [Header("--- CreateAccountPanel ---")]
    public GameObject m_CreatePanelObj;
    public InputField New_IdInputField;
    public InputField New_PwInputField;
    public InputField New_NickInputField;
    public Button m_CreateAccountBtn = null;
    public Button m_CancelBtn = null;

    [Header("--- Message ---")]
    public Text MessageText;
    float ShowMsTimer = 0.0f;

    string m_SvIdStr = "";

    // Start is called before the first frame update
    void Start()
    {
        GlobalValue.LoadGameData();
        NetworkMgr.Inst.ReadyNetworkMgr(this);  //������ �߰�
        
        //--- LoginPanel
        if (m_LoginBtn != null)
            m_LoginBtn.onClick.AddListener(LoginBtnClick);

        if (m_CreateAccOpenBtn != null)
            m_CreateAccOpenBtn.onClick.AddListener(OpenCreateAccBtn);

        //--- CreateAccountPanel
        if (m_CancelBtn != null)
            m_CancelBtn.onClick.AddListener(CancelBtnClick);

        if (m_CreateAccountBtn != null)
            m_CreateAccountBtn.onClick.AddListener(CreateAccountBtn);

        //--- üũ ��ư���� ���ÿ� ������ ���Ҵ� ������ ���� Id �ε��� ����
        string a_strId = PlayerPrefs.GetString("MySave_Id", "");
        if(PlayerPrefs.HasKey("MySave_Id") == false || a_strId == "")
        {
            SaveIdToggle.isOn = false;
        }
        else
        {
            SaveIdToggle.isOn = true;
            IdInputField.text = a_strId;
        }
        //--- üũ ��ư���� ���ÿ� ������ ���Ҵ� ������ ���� Id �ε��� ����
    }

    // Update is called once per frame
    void Update()
    {
        if(0.0f < ShowMsTimer)
        {
            ShowMsTimer -= Time.deltaTime;
            if(ShowMsTimer <= 0.0f)
            {
                MessageOnOff("", false);    //�޽��� ����
            }
        }//if(0.0f < ShowMsTimer)
    }

    void LoginBtnClick()
    {
        string a_IdStr = IdInputField.text;
        string a_PwStr = PwInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();

        if(string.IsNullOrEmpty(a_IdStr) == true ||
           string.IsNullOrEmpty(a_PwStr) == true)
        {
            MessageOnOff("Id, Pw�� ��ĭ ���� �Է��� �ּ���.");
            return;
        }

        if( !(3 <= a_IdStr.Length && a_IdStr.Length <= 20) )
        {
            MessageOnOff("Id�� 3���� �̻� 20���� ���Ϸ� �ۼ��� �ּ���.");
            return;
        }

        if( !(4 <= a_PwStr.Length && a_PwStr.Length <= 20) )
        {
            MessageOnOff("Pw�� 4���� �̻� 20���� ���Ϸ� �ۼ��� �ּ���.");
            return;
        }

        //StartCoroutine( LoginCo(a_IdStr, a_PwStr) );

        NetworkMgr.Inst.m_IdStrBuff = a_IdStr;
        NetworkMgr.Inst.m_PwStrBuff = a_PwStr;
        NetworkMgr.Inst.PushPacket(PacketType.Login);

    }// void LoginBtnClick()

    private void OpenCreateAccBtn()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(false);

        if (m_CreatePanelObj != null)
            m_CreatePanelObj.SetActive(true);
    }

    private void CancelBtnClick()
    {
        if (m_LoginPanelObj != null)
            m_LoginPanelObj.SetActive(true);

        if (m_CreatePanelObj != null)
            m_CreatePanelObj.SetActive(false);

        New_IdInputField.text   = "";
        New_PwInputField.text   = "";
        New_NickInputField.text = "";
    }

    private void CreateAccountBtn()
    {
        string a_IdStr = New_IdInputField.text;
        string a_PwStr = New_PwInputField.text;
        string a_NickStr = New_NickInputField.text;

        a_IdStr = a_IdStr.Trim();
        a_PwStr = a_PwStr.Trim();
        a_NickStr = a_NickStr.Trim();

        if (string.IsNullOrEmpty(a_IdStr) == true ||
            string.IsNullOrEmpty(a_PwStr) == true ||
            string.IsNullOrEmpty(a_NickStr) == true)
        {
            MessageOnOff("Id, Pw, ������ ��ĭ ���� �Է��� �ּ���.");
            return;
        }

        if (!(3 <= a_IdStr.Length && a_IdStr.Length <= 20))
        {
            MessageOnOff("Id�� 3���� �̻� 20���� ���Ϸ� �ۼ��� �ּ���.");
            return;
        }

        if (!(4 <= a_PwStr.Length && a_PwStr.Length <= 20))
        {
            MessageOnOff("Pw�� 4���� �̻� 20���� ���Ϸ� �ۼ��� �ּ���.");
            return;
        }

        if (!(2 <= a_NickStr.Length && a_NickStr.Length <= 20))
        {
            MessageOnOff("���� 2���� �̻� 20���� ���Ϸ� �ۼ��� �ּ���.");
            return;
        }

        //StartCoroutine(CreateAccCo(a_IdStr, a_PwStr, a_NickStr));

        NetworkMgr.Inst.m_IdStrBuff = a_IdStr;
        NetworkMgr.Inst.m_PwStrBuff = a_PwStr;
        NetworkMgr.Inst.m_NickStrBuff = a_NickStr;
        NetworkMgr.Inst.PushPacket(PacketType.CreateAccount);

    }//void CreateAccountBtn()

    public void MessageOnOff(string a_Msg = "", bool isOn = true)
    {
        if(isOn == true)
        {
            MessageText.text = a_Msg;
            MessageText.gameObject.SetActive(true);
            ShowMsTimer = 7.0f;
        }
        else
        {
            MessageText.text = "";
            MessageText.gameObject.SetActive(false);    
        }
    }//public void MessageOnOff(string a_Msg = "", bool isOn = true)

    public void LoginOkGoLobby(string a_IdStr)
    {
        m_SvIdStr = a_IdStr;
        if (SaveIdToggle.isOn == true)  //üũ ��ư�� ���� ������
        {
            PlayerPrefs.SetString("MySave_Id", m_SvIdStr);
        }
        else  //üũ ��ư�� ���� ������
        {
            PlayerPrefs.DeleteKey("MySave_Id");
        }

        SceneManager.LoadScene("Lobby");
    }
}
