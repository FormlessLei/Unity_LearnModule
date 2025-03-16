using UnityEngine;
using System.Collections.Generic;

public interface IUpdateable
{
    void OnUpdate(float dt);
}
public class UpdateableComponent : MonoBehaviour, IUpdateable
{
    public void OnUpdate(float dt)
    {
    }
}

public class GameLogicSingleton : SingletonComponent<GameLogicSingleton>
{
    List<IUpdateable> _updateableObjects = new List<IUpdateable>();

    public void RegisterUpdateObj(IUpdateable obj)
    {
        if (!_updateableObjects.Contains(obj))
            _updateableObjects.Add(obj);
    }

    public void UnregisterUpdateObj(IUpdateable obj)
    {
        if (_updateableObjects.Contains(obj))
            _updateableObjects.Remove(obj);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < _updateableObjects.Count; i++)
        {
            _updateableObjects[i].OnUpdate(dt);
        }
    }
}