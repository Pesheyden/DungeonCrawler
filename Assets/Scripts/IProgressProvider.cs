using System;
using System.Collections;
using UnityEngine;

public interface IProgressProvider
{
    public float GetProgress();
    public string GetProgressTitle();
}
