using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using JointStateMsg = RosMessageTypes.Sensor.JointStateMsg;

public class ur3subscriber : MonoBehaviour
{
    [System.Serializable]
    public class JointMapping
    {
        public string rosName;
        public ArticulationBody body;
        public float offsetDeg = 0f;
        public float sign = 1f;
    }

    public JointMapping[] joints =
    {
        new() { rosName = "shoulder_pan_joint"  },
        new() { rosName = "shoulder_lift_joint" },
        new() { rosName = "elbow_joint"         },
        new() { rosName = "wrist_1_joint"       },
        new() { rosName = "wrist_2_joint"       },
        new() { rosName = "wrist_3_joint"       },
        new() { rosName = "finger_joint"               },
        new() { rosName = "left_inner_knuckle_joint"   },
        new() { rosName = "left_inner_finger_joint"    },
        new() { rosName = "right_outer_knuckle_joint"  },
        new() { rosName = "right_inner_knuckle_joint"  },
        new() { rosName = "right_inner_finger_joint"   },
    };

    public string topicName = "/joint_states";
    public bool debugLogFirstMessage = true;

    [Tooltip("Root of the UR3 hierarchy used to auto-find ArticulationBodies if a slot's body is null. Defaults to this GameObject.")]
    public Transform robotRoot;

    private Dictionary<string, JointMapping> jointMap;
    private ROSConnection ros;
    private bool loggedFirst;

    void Reset()
    {
        joints = new[]
        {
            new JointMapping { rosName = "shoulder_pan_joint"  },
            new JointMapping { rosName = "shoulder_lift_joint" },
            new JointMapping { rosName = "elbow_joint"         },
            new JointMapping { rosName = "wrist_1_joint"       },
            new JointMapping { rosName = "wrist_2_joint"       },
            new JointMapping { rosName = "wrist_3_joint"       },
            new JointMapping { rosName = "finger_joint"               },
            new JointMapping { rosName = "left_inner_knuckle_joint"   },
            new JointMapping { rosName = "left_inner_finger_joint"    },
            new JointMapping { rosName = "right_outer_knuckle_joint"  },
            new JointMapping { rosName = "right_inner_knuckle_joint"  },
            new JointMapping { rosName = "right_inner_finger_joint"   },
        };
    }

    void OnValidate()
    {
        if (joints == null) return;
        foreach (var j in joints)
        {
            if (j != null && j.sign == 0f) j.sign = 1f;
        }
    }

    static readonly Dictionary<string, float> Ur3DefaultOffsetDeg = new()
    {
        { "shoulder_lift_joint", 90f },
    };

    void Start()
    {
        Dictionary<string, ArticulationBody> bodiesByName = BuildBodyIndex();

        jointMap = new Dictionary<string, JointMapping>();
        foreach (var j in joints)
        {
            if (j == null || string.IsNullOrEmpty(j.rosName)) continue;

            if (j.body == null)
            {
                if (bodiesByName.TryGetValue(j.rosName, out ArticulationBody found))
                {
                    j.body = found;
                    Debug.Log($"[ur3subscriber] Auto-bound '{j.rosName}' to '{found.name}'.");
                }
                else
                {
                    Debug.LogWarning($"[ur3subscriber] Joint '{j.rosName}' has no ArticulationBody assigned and none found under robotRoot.");
                    continue;
                }
            }

            if (j.sign == 0f) j.sign = 1f;
            if (j.offsetDeg == 0f && Ur3DefaultOffsetDeg.TryGetValue(j.rosName, out float def))
                j.offsetDeg = def;
            jointMap[j.rosName] = j;
        }

        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<JointStateMsg>(topicName, Callback);
        Debug.Log($"[ur3subscriber] Subscribed to {topicName} with {jointMap.Count} joints mapped.");
    }

    // Maps Unity ArticulationBody names (usually the URDF child link) → ROS joint name.
    static readonly Dictionary<string, string> Ur3LinkToJoint = new()
    {
        { "shoulder_link",       "shoulder_pan_joint"         },
        { "upper_arm_link",      "shoulder_lift_joint"        },
        { "forearm_link",        "elbow_joint"                },
        { "wrist_1_link",        "wrist_1_joint"              },
        { "wrist_2_link",        "wrist_2_joint"              },
        { "wrist_3_link",        "wrist_3_joint"              },
        { "left_outer_knuckle",  "finger_joint"               },
        { "left_inner_knuckle",  "left_inner_knuckle_joint"   },
        { "left_inner_finger",   "left_inner_finger_joint"    },
        { "right_outer_knuckle", "right_outer_knuckle_joint"  },
        { "right_inner_knuckle", "right_inner_knuckle_joint"  },
        { "right_inner_finger",  "right_inner_finger_joint"   },
    };

    Dictionary<string, ArticulationBody> BuildBodyIndex()
    {
        Transform root = robotRoot != null ? robotRoot : transform;
        var bodies = root.GetComponentsInChildren<ArticulationBody>(includeInactive: true);
        var map = new Dictionary<string, ArticulationBody>();

        foreach (var b in bodies)
        {
            string n = b.name;
            if (!map.ContainsKey(n)) map[n] = b;

            if (Ur3LinkToJoint.TryGetValue(n, out string jointName) && !map.ContainsKey(jointName))
                map[jointName] = b;

            string alt = n.EndsWith("_link") ? n[..^"_link".Length] + "_joint" : null;
            if (alt != null && !map.ContainsKey(alt)) map[alt] = b;
        }
        return map;
    }

    void Callback(JointStateMsg msg)
    {
        if (msg.name == null || msg.position == null) return;

        if (debugLogFirstMessage && !loggedFirst)
        {
            loggedFirst = true;
            Debug.Log($"[ur3subscriber] First message received. names=[{string.Join(",", msg.name)}] count={msg.position.Length}");
        }

        int count = Mathf.Min(msg.name.Length, msg.position.Length);
        for (int i = 0; i < count; i++)
        {
            if (!jointMap.TryGetValue(msg.name[i], out JointMapping j)) continue;

            float rosDeg = Mathf.Rad2Deg * (float)msg.position[i];
            float unityDeg = j.sign * rosDeg + j.offsetDeg;

            ArticulationDrive aDrive = j.body.xDrive;
            aDrive.target = unityDeg;
            j.body.xDrive = aDrive;
        }
    }
}
