using System.Collections.Generic;
using UnityEngine;


public class CreateWorkMessage : Message { }
public class WorkCreator : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(500,500,500,500));
        if (GUILayout.Button("CreatorWork"))
        {
            MessageSystem.Instance.QueueMessage(new CreateWorkMessage());
        }
        GUILayout.EndArea();
    }
}


public class WorkDoneMessage : Message
{
    public readonly int workValue;
    public readonly string workName;
    public WorkDoneMessage(int workValue, string workName)
    {
        this.workValue = workValue;
        this.workName = workName;
    }
}

public class WorkDoneListener : MonoBehaviour
{
    private void Start()
    {
        MessageSystem.Instance.AttachListener(typeof(WorkDoneMessage), HandleWorkDone);
        // 最好的方法是，为每种消息类型定义一个唯一的方法。
    }

    private bool HandleWorkDone(Message message)
    {
        WorkDoneMessage catchMsg = message as WorkDoneMessage;
        Debug.Log($"WorkDone: {catchMsg.workName} has been completed, generating a value of {catchMsg.workValue}.");
        return true;
    }

    private void OnDestroy()
    {
        if (MessageSystem.IsAlive)
        {
            MessageSystem.Instance.DetachListener(typeof(WorkDoneMessage), this.HandleWorkDone);
        }
    }
}


public class WorkManager : MonoBehaviour
{
    private List<string> _works = new List<string>();

    private void Start()
    {
        MessageSystem.Instance.AttachListener(typeof(CreateWorkMessage), this.HandleCreateWork);
    }

    private bool HandleCreateWork(Message message)
    {
        var catchMsg = message as CreateWorkMessage;

        string[] names = { "Code", "Read", "exercise" };

        string workName = names[Random.Range(0, names.Length)];
        int workValue = Random.Range(0, 101);
        _works.Add(workName);
        MessageSystem.Instance.QueueMessage(new WorkDoneMessage(workValue, workName));
        return true;
    }

    private void OnDestroy()
    {
        if (MessageSystem.IsAlive)
        {
            MessageSystem.Instance.DetachListener(typeof(CreateWorkMessage), this.HandleCreateWork);
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("AddWorkComponent"))
        {
            gameObject.AddComponent<WorkCreator>();
            gameObject.AddComponent<WorkDoneListener>();
        }
    }
}
