# UR_Unity_Sim

Stream Universal Robots joint states from ROS into a Unity scene, driving a UR3 `ArticulationBody` rig in real time.

## Requirements

- **Unity Editor:** `2022.3.62f3`
- **ROS side:** a running publisher on `/joint_states` (`sensor_msgs/JointState`)

## Unity Packages

Install via **Window → Package Manager → + → Add package from git URL**:

| Package | Git URL |
| --- | --- |
| URDF Importer | `https://github.com/Unity-Technologies/URDF-Importer.git?path=/com.unity.robotics.urdf-importer` |
| ROS TCP Connector | `https://github.com/Unity-Technologies/ROS-TCP-Connector.git?path=/com.unity.robotics.ros-tcp-connector` |

## Getting Started

1. Open the project in Unity `2022.3.62f3`.
2. Install the two packages above.
3. Configure the ROS TCP endpoint under **Robotics → ROS Settings**.
4. Import or open the UR3 URDF so the articulation hierarchy is in the scene.
5. Attach `ur3subscriber` to the UR3 root and press **Play**.

## Scripts

- `Assets/RequiredAssets/Scripts/ur3subscriber.cs` — subscribes to `/joint_states`, auto-binds the six UR3 joints (`shoulder_pan`, `shoulder_lift`, `elbow`, `wrist_1/2/3`) to their `ArticulationBody` counterparts, and writes target angles each message.