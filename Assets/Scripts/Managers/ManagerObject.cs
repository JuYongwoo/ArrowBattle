using UnityEngine;

public class ManagerObject : MonoBehaviour
{
    static public ManagerObject instance;

    public AudioManager audioManager = new AudioManager();
    public ResourceManager resourceManager = new ResourceManager();
    public InputManager inputManager = new InputManager();
    public EventManager eventManager = new EventManager();
    public SkillDataBaseManager skillInfoManager = new SkillDataBaseManager();
    public PoolManager poolManager = new PoolManager();


    private void Awake()
    {

        MakeInstance();
        resourceManager.OnAwake();
        audioManager.OnStart();
        Screen.SetResolution(1600, 900, false);

    }

    private void OnDestroy()
    {
        audioManager.OnDestroy();
    }

    private void Update()
    {
        inputManager.OnUpdate();
    }

    private void MakeInstance()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

}
