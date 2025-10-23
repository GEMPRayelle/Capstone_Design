using UnityEngine;

//Manager들 총괄클래스
public class Managers : Singleton<Managers>
{
    #region Core
    private ResourceManager _resource = new ResourceManager();
    private DataManager _data = new DataManager();
    private PoolManager _pool = new PoolManager();
    private SceneManagerEx _scene = new SceneManagerEx();
    private SoundManager _sound = new SoundManager();
    private UIManager _ui = new UIManager();

    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static DataManager Data { get { return Instance?._data; } }
    public static PoolManager Pool { get { return Instance?._pool; } }
    public static SceneManagerEx Scene { get { return Instance?._scene; } }
    public static SoundManager Sound { get { return Instance?._sound; } }
    public static UIManager UI { get { return Instance?._ui; } }
    #endregion

    #region Contents
    private GameManager _game = new GameManager();
    private ObjectManager _object = new ObjectManager();
    private MapManager _map = new MapManager();
    private TurnManager _turn = new TurnManager();
    private ListenerManager _listener = new ListenerManager();
    private ControllerManager _controller = new ControllerManager();
    private CutSceneManager _cutscene = new CutSceneManager();

    public static GameManager Game { get { return Instance?._game; } }
    public static ObjectManager Object { get { return Instance?._object; } }
    public static MapManager Map { get { return Instance?._map; } }
    public static TurnManager Turn { get { return Instance?._turn; } }
    public static ListenerManager Listener { get { return Instance?._listener; } }
    public static ControllerManager Controller { get { return Instance?._controller; } }
    public static CutSceneManager CutScene { get { return Instance?._cutscene; } }
    #endregion

}
