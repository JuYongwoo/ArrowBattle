using UnityEngine;

public class ManagerObject : MonoBehaviour
{
    static public ManagerObject instance;

    public AudioManager audioM = new AudioManager();
    public ResourceManager resourceManager = new ResourceManager();
    public InputManager inputM = new InputManager();
    public ActionManager actionManager = new ActionManager();
    public SkillDataBaseManager skillInfoM = new SkillDataBaseManager();


    private void Awake()
    {

        makeInstance();

        
        
        
        resourceManager.OnAwake();

        audioM.OnAwake();
    }
    private void Update()
    {
        inputM.OnUpdate();
    }

    private void makeInstance()
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
