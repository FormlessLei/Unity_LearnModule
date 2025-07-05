using Unity.VisualScripting;
using UnityEngine;

public class ResetPooledRigidbodyComponent : MonoBehaviour, IPoolableComponent
{
    [SerializeField] private Rigidbody _body;

    public void Spawned()
    {
    }

    // 清理组件信息的最佳时机是在回收时。如果在生成时，不同组件的生成时机不确定，可能导致潜在冲突。
    public void Despawned()
    {
        if (_body == null)
        {
            _body = GetComponent<Rigidbody>();
            if (_body == null)
            {
                return;
            }
        }

        _body.linearVelocity = Vector3.zero;
        _body.angularVelocity = Vector3.zero;
    }


}

public class PoolableTestMessageListener : MonoBehaviour, IPoolableComponent
{
    public void Spawned()
    {
        MessageSystem.Instance.AttachListener(typeof(Message), this.HandleMyCustomMessage);
    }

    private bool HandleMyCustomMessage(Message msg)
    {
        Debug.Log($"Got the message!");
        return true;
    }

    public void Despawned()
    {
        if (MessageSystem.IsAlive)
            MessageSystem.Instance.DetachListener(typeof(Message), this.HandleMyCustomMessage);
    }
}