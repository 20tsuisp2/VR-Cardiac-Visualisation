using UnityEngine;

public class GazeInteractionModeManager : MonoBehaviour
{
    public GazeMoveObject moveObject;
    public GazeRotateObjectYaw rotateYawObject;
    public GazeRotateObjectRoll rotateRollObject;

    public void ToggleMoveModeExclusive()
    {
        if (rotateYawObject != null && rotateYawObject.IsRotateMode())
            rotateYawObject.ToggleRotateMode();

        if (rotateRollObject != null && rotateRollObject.IsRotateMode())
            rotateRollObject.ToggleRotateMode();

        if (moveObject != null)
            moveObject.ToggleMoveMode();
    }

    public void ToggleYawModeExclusive()
    {
        if (moveObject != null && moveObject.IsMoveMode())
            moveObject.ToggleMoveMode();

        if (rotateRollObject != null && rotateRollObject.IsRotateMode())
            rotateRollObject.ToggleRotateMode();

        if (rotateYawObject != null)
            rotateYawObject.ToggleRotateMode();
    }

    public void ToggleRollModeExclusive()
    {
        if (moveObject != null && moveObject.IsMoveMode())
            moveObject.ToggleMoveMode();

        if (rotateYawObject != null && rotateYawObject.IsRotateMode())
            rotateYawObject.ToggleRotateMode();

        if (rotateRollObject != null)
            rotateRollObject.ToggleRotateMode();
    }
}
