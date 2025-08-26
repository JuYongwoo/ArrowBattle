using UnityEngine;

public class ManagerObject : MonoBehaviour
{
    static public ManagerObject instance;

    static public AudioManager audio = new AudioManager();
    static public InputManager input = new InputManager();
    static public StatManager characterStat = new StatManager();

    private void Awake()
    {
        //if (instance != null && instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        instance = this;
        //DontDestroyOnLoad(gameObject);
        characterStat.OnAwake();
        audio.OnAwake();
        input.OnAwake();
    }
    void Start()
    {
        Screen.SetResolution(1600, 900, false);
        audio.onStart();

    }

    void Update()
    {
        input.OnUpdate();
    }


}
