using NUnit.Framework.Internal;
using System;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class TestStartup : MonoBehaviour
{
    public int commonTestNum1m = 1000000;
    public int commonTestNum10m = 10000000;
    public int commonTestNum10k = 10000;
    public int commonTestNum50k = 50000;

    /*
     * CPU Intel(R) Core(TM) i5-12400F
     * 内存 32.0GB
     */

    private void OnGUI()
    {
        GUILayout.Label("Check in Console");
        if (GUILayout.Button("GetComponent"))
        {
            GetComponentTest(commonTestNum1m);
        }
        if (GUILayout.Button("InsTransformParentInCreate"))
        {
            InsTransformParent(commonTestNum10k, false);
        }
        if (GUILayout.Button("InsTransformParentSepartely"))
        {
            InsTransformParent(commonTestNum10k, true);
        }
        if (GUILayout.Button("InsTransform"))
        {
            InsTransformCapacity(commonTestNum50k, false);
        }
        if (GUILayout.Button("InsTransformWithCapacity"))
        {
            InsTransformCapacity(commonTestNum50k, true);
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

        if (GUILayout.Button("UpdateGetComponent"))
        {
            AddUpdateGetComponent(commonTestNum10k, false);
        }
        if (GUILayout.Button("UpdateCacheComponent"))
        {
            AddUpdateGetComponent(commonTestNum10k, true);
        }

        if (GUILayout.Button("CheckGOEqualWith=="))
        {
            CheckEqual(commonTestNum10m, false);
        }
        if (GUILayout.Button("CheckGOEqualWithRef"))
        {
            CheckEqual(commonTestNum10m, true);
        }

        if (GUILayout.Button("CheckTagWith=="))
        {
            CheckTag(commonTestNum10m, false);
        }
        if (GUILayout.Button("CheckTagWithCompare"))
        {
            CheckTag(commonTestNum10m, true);
        }
    }

    #region Fold
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
        //GetComponentTest(string) finished:303.00ms total,0.000303ms for 1000000 tests.

        using (new CustomTimer("GetComponentTest<CompName>", testNum))
        {
            for (int i = 0; i < testNum; i++)
            {
                test = GetComponent<TestComponent>();
            }
        }
        //GetComponentTest<CompName> finished:125.00ms total,0.000125ms for 1000000 tests.

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
        // 每一个空Update，增加0.0002382毫秒，0.2382微秒，238纳秒
    }

    private void AddUpdateGetComponent(int testNum, bool withCache)
    {
        Transform root = new GameObject("CreateRoot").transform;

        if (withCache)
        {
            for (int i = 0; i < testNum; i++)
            {
                new GameObject(i.ToString(), typeof(DataComponent), typeof(UpdateCacheComponent)).transform.SetParent(root);
            }
            return;
        }
        //1万，Update函数耗时0.51ms。obj内存从8.51k到48.51k--此统计应该和脚本中的缓存不同

        for (int i = 0; i < testNum; i++)
        {
            new GameObject(i.ToString(), typeof(DataComponent), typeof(UpdateGetComponent)).transform.SetParent(root);
        }
        // 1万，Update函数耗时7.95ms，其中GetComponent函数耗时6.68ms。obj内存从8.55k到48.55k--此统计应该和脚本中的缓存不同
        // 每一个GetComponent，增加0.000668毫秒，0.668微秒，668纳秒
    }

    private void CheckEqual(int testNum, bool isRefEqu)
    {
        if (isRefEqu)
        {
            using (new CustomTimer("CheckEqual_ReferenceEquals", testNum))
            {
                for (int i = 0; i < testNum; i++)
                {
                    if (!System.Object.ReferenceEquals(gameObject, null)) ;
                }
            }
            return;
            //CheckEqual_ReferenceEquals finished:18829.00ms total,0.001883ms for 10000000 tests.
        }

        using (new CustomTimer("CheckEqual==", testNum))
        {
            for (int i = 0; i < testNum; i++)
            {
                if (gameObject == null) ;
            }
        }
        //CheckEqual== finished:24027.00ms total,0.002403ms for 10000000 tests.
        // GC上没有看出区别。Ref比==少调用操作重载符Object.op_Equality()，节约20%时间。
    }

    private void CheckTag(int testNum, bool isCompare)
    {
        if (isCompare)
        {
            using (new CustomTimer("CheckTag_ReferenceEquals", testNum))
            {
                for (int i = 0; i < testNum; i++)
                {
                    if (gameObject.CompareTag("Player")) ;
                }
            }
            return;
            //CheckTag_ReferenceEquals finished:31300.00ms total,0.003130ms for 10000000 tests.
            //GC为0
        }

        using (new CustomTimer("CheckTag==", testNum))
        {
            for (int i = 0; i < testNum; i++)
            {
                if (gameObject.tag == "Player") ;
            }
        }
        //CheckTag== finished:38199.00ms total,0.003820ms for 10000000 tests.
        //GC为362.4MB！！！
        //十万次测试，GC为3.6MB。一次36B？测试字符串长度：Player，PlayerXXXXXXXXXFVW，GC相同。
    }
    #endregion

    private void InsTransformParent(int testNum, bool setParentSepartely)
    {
        Transform root = new GameObject("CreateRoot").transform;
        GameObject go = new GameObject("Model");
        if (setParentSepartely)
        {
            using (new CustomTimer("InsTransformParentSepartely", testNum))
            {

                for (int i = 0; i < testNum; i++)
                {
                    Instantiate(go).transform.SetParent(root);
                }
            }
            //InsTransformParentSepartely finished:85.00ms total,0.008500ms for 10000 tests.
            return;
        }
        using (new CustomTimer("InsTransformParentOneStep", testNum))
        {
            for (int i = 0; i < testNum; i++)
            {
                Instantiate(go, root);
            }
            //InsTransformParentOneStep finished:45.00ms total,0.004500ms for 10000 tests.
        }
        //每次差0.004ms，4微秒。
    }

    private void InsTransformCapacity(int testNum, bool isUseCapacity)
    {
        Transform root = new GameObject("CreateRoot").transform;
        GameObject go = new GameObject("Model");
        Debug.Log(root.hierarchyCapacity);
        Debug.Log(root.hierarchyCount);
        if (isUseCapacity)
        {
            root.transform.hierarchyCapacity = (int)(testNum*1.1f);
            using (new CustomTimer("InsTransformWithCapacity", testNum))
            {

                for (int i = 0; i < testNum; i++)
                {
                    Instantiate(go, root);
                }
            }
            //InsTransformWithCapacity finished:226.00ms total,0.004520ms for 50000 tests.
            Debug.Log(root.hierarchyCapacity);// 55000
            Debug.Log(root.hierarchyCount);// 50001
            return;
        }
        using (new CustomTimer("InsTransform", testNum))
        {
            for (int i = 0; i < testNum; i++)
            {
                Instantiate(go, root);
            }
            //InsTransform finished:236.00ms total,0.004720ms for 50000 tests.
        }
        Debug.Log(root.hierarchyCapacity);// 98302
        Debug.Log(root.hierarchyCount);// 50001
        // 无法确定时间上的影响，但内存上应该有影响？Proflier上看不出来。
    }
}
