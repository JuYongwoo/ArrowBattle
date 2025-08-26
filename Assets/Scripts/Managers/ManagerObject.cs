using UnityEngine;

public class ManagerObject : MonoBehaviour
{
    static public ManagerObject instance;

    static public AudioManager audioM = new AudioManager();
    static public InputManager inputM = new InputManager();
    static public SkillDataBaseManager skillInfoM = new SkillDataBaseManager();

    private void Awake()
    {
        //if (instance != null && instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        instance = this;
        //DontDestroyOnLoad(gameObject);
        skillInfoM.OnAwake();
        audioM.OnAwake();
        inputM.OnAwake();
    }
    void Start()
    {
        Screen.SetResolution(1600, 900, false);
        audioM.onStart();

    }

    void Update()
    {
        inputM.OnUpdate();
    }


}
