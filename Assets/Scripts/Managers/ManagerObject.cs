using UnityEngine;

public class ManagerObject : MonoBehaviour
{
    static public ManagerObject instance;

    static public AudioManager audioM = new AudioManager();
    static public InputManager inputM = new InputManager();
    static public SkillDataBaseManager skillInfoM = new SkillDataBaseManager();
    static public GameMode gameMode = new GameMode();


    private void Awake()
    {
        //if (instance != null && instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        instance = this;
        //DontDestroyOnLoad(gameObject);
        gameMode.OnAwake();
        skillInfoM.OnAwake();
        audioM.OnAwake();
    }
    private void Start()
    {
        gameMode.OnStart();
    }

    private void Update()
    {
        inputM.OnUpdate();
    }


}
