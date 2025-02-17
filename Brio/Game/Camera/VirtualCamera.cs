﻿using FFXIVClientStructs.FFXIV.Client.Game.Control;
using MessagePack;
using System.Numerics;

namespace Brio.Game.Camera;

[MessagePackObject(keyAsPropertyName: true)]
public unsafe class VirtualCamera
{
    public VirtualCamera() { }
    public VirtualCamera(int cameraID)
    {
        CameraID = cameraID;

        ResetCamera();
    }

    public FreeCamValues FreeCamValues { get; } = new FreeCamValues();

    [IgnoreMember] public BrioCamera* BrioCamera => (BrioCamera*)CameraManager.Instance()->GetActiveCamera();

    public unsafe bool IsOverridden => DisableCollision || DelimitCamera || PositionOffset != Vector3.Zero
    | PivotRotation != 0 | FoV != 0 | Pan != Vector2.Zero | BrioCamera->Camera.Distance != 2.5f;

    [IgnoreMember] public bool IsActiveCamera { get; set; } = false;
    public bool IsFreeCamera { get; set; } = false;

    [IgnoreMember] public int CameraID { get; private set; } = -1;

    public Vector3 RealPosition => BrioCamera->GetPosition();

    public Vector3 Position = Vector3.Zero;
    public Vector3 PositionOffset = Vector3.Zero;
    public Vector3 Rotation = Vector3.Zero;

    public float PivotRotation
    {
        get => IsActiveCamera ? BrioCamera->Rotation : (field);
        set
        {
            _ = IsActiveCamera ? (BrioCamera->Rotation = field = value) : (field = value);
        }
    }
    public float Zoom
    {
        get => IsActiveCamera ? BrioCamera->Camera.Distance : (field);
        set
        {
            _ = IsActiveCamera ? (BrioCamera->Camera.Distance = field = value) : (field = value);
        }
    }
    public float FoV
    {
        get => IsActiveCamera ? BrioCamera->FoV : (field);
        set
        {
            _ = IsActiveCamera ? (BrioCamera->FoV = field = value) : (field = value);
        }
    }

    public Vector2 Pan
    {
        get => IsActiveCamera ? BrioCamera->Pan : (field);
        set
        {
            _ = IsActiveCamera ? (BrioCamera->Pan = field = value) : (field = value);
        }
    }
    public Vector2 Angle
    {
        get => IsActiveCamera ? BrioCamera->Angle : (field);
        set
        {
            _ = IsActiveCamera ? (BrioCamera->Angle = field = value) : (field = value);
        }
    }

    public bool DisableCollision = false;

    private Vector2? _originalZoomLimits { get; set; } = null;
    bool delimitCameraHasOverride = false;
    [IgnoreMember]
    public bool DelimitCamera
    {
        get => _originalZoomLimits.HasValue;
        set
        {
            if(IsActiveCamera == false)
                return;

            if(value)
                DelimitCameraStart();
            else
                DelimitCameraStop();

            delimitCameraHasOverride = value;
        }
    }

    private void DelimitCameraStop()
    {
        if(_originalZoomLimits.HasValue)
        {
            BrioCamera->Camera.MinDistance = _originalZoomLimits.Value.X;
            BrioCamera->Camera.MaxDistance = _originalZoomLimits.Value.Y;

            if(BrioCamera->Camera.Distance < BrioCamera->Camera.MinDistance)
                BrioCamera->Camera.Distance = BrioCamera->Camera.MinDistance;

            _originalZoomLimits = null;
        }
    }
    private void DelimitCameraStart()
    {
        _originalZoomLimits = new Vector2(BrioCamera->Camera.MinDistance, BrioCamera->Camera.MaxDistance);

        BrioCamera->Camera.MinDistance = 0f;
        BrioCamera->Camera.MaxDistance = 500f;
    }

    public void ActivateCamera()
    {
        if(IsActiveCamera)
            return;

        LoadCameraState();

        IsActiveCamera = true;
    }
    public void DeactivateCamera()
    {
        if(IsActiveCamera is false)
            return;
        IsActiveCamera = false;

        SaveCameraState();
    }

    public unsafe void ResetCamera()
    {
        PositionOffset = Vector3.Zero;

        DisableCollision = false;
        DelimitCamera = false;

        PivotRotation = 0;
        Zoom = 2.5f;
        FoV = 0f;
        Angle = Vector2.Zero;
        Pan = Vector2.Zero;
    }

    public void SaveCameraState()
    {
        PivotRotation = BrioCamera->Rotation;
        Zoom = BrioCamera->Camera.Distance;
        FoV = BrioCamera->FoV;

        Angle = BrioCamera->Angle;
        Pan = BrioCamera->Pan;
    }
    public void LoadCameraState()
    {
        BrioCamera->Rotation = PivotRotation;
        BrioCamera->Camera.Distance = Zoom;
        BrioCamera->FoV = FoV;

        BrioCamera->Angle = Angle;
        BrioCamera->Pan = Pan;

        DelimitCamera = delimitCameraHasOverride;
    }

    public void SetCameraID(int id)
    {
        CameraID = id;
    }

    public void ToFreeCam()
    {
        if(Position == Vector3.Zero)
            Position = RealPosition;
        IsFreeCamera = true;
    }
}

[MessagePackObject(keyAsPropertyName: true)]
public class FreeCamValues
{
    public bool IsMovementEnabled = false;
    public bool Move2D = false;

    public float MouseSensitivity = 0f;
    public float MovementSpeed = 0f;

    public bool DelimitAngle = false;
}
