using WorldWeaver.Managers;

namespace WorldWeaver.Data;

public abstract class WeaverSceneDataHandler
{
    public abstract string ModIdentifier { get; }

    public abstract SceneData? GetSceneData();

    public WeaverSceneDataHandler()
    {
        if (string.IsNullOrEmpty(ModIdentifier))
            return;
            
        Init();
    }
    
    public virtual void Init()
    {
        WeaverDataManager.TryAddDataHandler(this);
    }
}