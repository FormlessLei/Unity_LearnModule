using System;
using System.Linq.Expressions;
using UnityEngine;

public class TestStartup : MonoBehaviour
{
    public int commonTestNum1m = 1000000;
    public int commonTestNum10m = 10000000;
    public int commonTestNum10k = 10000;
    public int commonTestNum50k = 50000;
    private void OnGUI()
    {
        GUILayout.Label("Check in Console");
        if (GUILayout.Button("GetComponent"))
        {
            GetComponentTest(commonTestNum1m);
        }

        GUILayout.Space(30);
        GUILayout.Label("Check in Profiler");

        if (GUILayout.Button("AddEmpty"))
        {
            AddEmpty(commonTestNum50k, false);
        }
        if (GUILayout.Button("AddCallbackEmpty"))
        {
            AddEmpty(commonTestNum50k, true);
        }
    }


    private void GetComponentTest(int testNum)
    {
        TestComponent test;
        using (new CustomTimer("GetComponentTest(string)", testNum))
        {
            for (int i = 0; i < testNum; i++)
            {
                test = (TestComponent)GetComponent("TestComponent");
            }
        }
        //GetComponentTest(string) finished:303.00ms total,0.000303 for 1000000 tests.

        using (new CustomTimer("GetComponentTest<CompName>", testNum))
        {
            for (int i = 0; i < testNum; i++)
            {
                test = GetComponent<TestComponent>();
            }
        }
        //GetComponentTest<CompName> finished:125.00ms total,0.000125 for 1000000 tests.

        using (new CustomTimer("GetComponentTest(typeof)", testNum))
        {
            for (int i = 0; i < testNum; i++)
            {
                test = (TestComponent)GetComponent(typeof(TestComponent));
            }
        }
        //GetComponentTest(typeof) finished:343.00ms total,0.000343 for 1000000 tests.
    }

    private void AddEmpty(int componentNum, bool withCallback)
    {
        Transform root = new GameObject("CreateRoot").transform;

        if (withCallback)
        {
            using (new CustomTimer("EmptyCallbackComponent", componentNum))
            {
                for (int i = 0; i < componentNum; i++)
                {
                    new GameObject(i.ToString(), typeof(EmptyCallbackComponent)).transform.SetParent(root);
                }
            }
            // 5万个空Update回调，cpu的Update耗时为13.3ms，PlayerLoop耗时13.63ms
            return;
        }

        using (new CustomTimer("EmptyClassComponent", componentNum))
        {
            for (int i = 0; i < componentNum; i++)
            {
                new GameObject(i.ToString(), typeof(EmptyClassComponent)).transform.SetParent(root);
            }
        }
        // 5万个无Update的空组件，cpu无Update耗时，PlayerLoop耗时1.72ms
        // 每一个空Update，增加0.0002382毫秒，0.2382微秒，238纳秒？
    }
}
