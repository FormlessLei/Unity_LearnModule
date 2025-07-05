public class TestObject : IPoolableObject
{
    public void New()
    {

    }

    public void Respawn()
    {

    }
}

public class TestObjectExample
{
    public static ObjectPool<TestObject> _objectPool=new ObjectPool<TestObject>();

}

