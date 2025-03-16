using UnityEngine;

public class TestComponent : MonoBehaviour
{

}

public class EmptyClassComponent : MonoBehaviour
{
}
public class EmptyCallbackComponent : MonoBehaviour
{
    private void Update() { }
}

public class DataComponent : MonoBehaviour
{
    public int data;
}

public class UpdateCacheComponent : MonoBehaviour
{
    private DataComponent _dc;
    private void Awake()
    {
        _dc = GetComponent<DataComponent>();
    }
    private void Update()
    {
        _dc.data = 1;
    }
}

public class UpdateGetComponent : MonoBehaviour
{
    private void Update()
    {
        var dc = GetComponent<DataComponent>();
        var dc2 = GetComponent<DataComponent>();
        dc.data = 1;
    }
}