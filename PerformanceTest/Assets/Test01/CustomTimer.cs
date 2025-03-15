using System;
using System.Diagnostics;
using UnityEngine;

public class CustomTimer : IDisposable
{
    private string _timerName;
    private int _numTests;
    private Stopwatch _watch;


    public CustomTimer(string timerName, int numTests)
    {
        _timerName = timerName;
        _numTests = numTests;
        if (_numTests <= 0) _numTests = 1;
        _watch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _watch.Stop();
        float ms = _watch.ElapsedMilliseconds;
        UnityEngine.Debug.Log($"{_timerName} finished:{ms:0.00}ms total,{ms / _numTests:0.000000}ms for {_numTests} tests.");
    }
}
