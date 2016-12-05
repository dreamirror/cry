using UnityEngine;
using System.Collections;

public class SH_SystemInfo : MonoBehaviour
{
    public string bundle_identifier;
    public string bundle_version;

    static public SH_SystemInfo Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }
}
