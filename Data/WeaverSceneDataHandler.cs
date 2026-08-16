using TeamCherry.SharedUtils;
using WorldWeaver.Managers;

namespace WorldWeaver.Data;

public abstract class WeaverSceneDataHandler : IIncludeVariableExtensions
{
    public abstract string ModIdentifier { get; }

    public abstract SceneData? GetSceneData();

    public string LastSetFieldName = string.Empty;

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

    public virtual void OnUpdatedVariable(string variableName)
    {
        LastSetFieldName = variableName;
    }
}