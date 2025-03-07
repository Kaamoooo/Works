using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class Cloud : MonoBehaviour
{
    [FormerlySerializedAs("material")] public Material cloudPostProcess;
    private Transform _cloudTransform;
    void Start()
    {
        _cloudTransform = this.transform;
    }

    void Update()
    {
        cloudPostProcess.SetVector("_CloudPosition", _cloudTransform.position);
        cloudPostProcess.SetVector("_CloudScale", _cloudTransform.localScale);
    }
}
