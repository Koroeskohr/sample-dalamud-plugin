using Dalamud.Bindings.ImGui;
using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using TargetArrow.Windows;
using System;
using System.Numerics;

namespace TargetArrow
{
    class TargetTracker : IDisposable
    {
        private DebugWindow debugWindow;
        private string arrowImagePath;
        private string goatImagePath;

        public TargetTracker(
            DebugWindow debugWindow,
            string arrowImagePath,
            string goatImagePath
            )
        {
            this.debugWindow = debugWindow;
            this.arrowImagePath = arrowImagePath;
            this.goatImagePath = goatImagePath;

            debugWindow.OnDebug += Debug;
        }

        public void Dispose()
        {
            debugWindow.OnDebug -= Debug;
        }

        public void DrawArrow()
        {
            var target = Plugin.TargetManager.Target;
            if (target == null)
                return;

            var sprite = Plugin.TextureProvider.GetFromFile(arrowImagePath).GetWrapOrDefault();
            if (sprite == null)
            {
                Plugin.Log.Error($"No sprite found at path {arrowImagePath}");
                return;
            }

            var angle = AngleToTarget();

            var center = new Vector2(700, 500);
            var size = new Vector2(64, 54);
            var corners = VectorMath.RotateQuad(center, size, angle);

            var drawList = ImGui.GetBackgroundDrawList();

            drawList.AddImageQuad(
                sprite.Handle,
                corners[0],
                corners[1],
                corners[2],
                corners[3]
            );
        }

        unsafe void Debug()
        {
            var camera = GetCamera();
            ImGui.Text("Camera HV");
            ImGui.Text($"{CameraDebug(camera)}");

            ImGui.Text("Camera DirV Min Max");
            ImGui.Text($"{CameraDirVMaxDebug(camera)}");

            ImGui.Text("Camera Look Vector");
            ImGui.Text($"{GetCameraLookVector(camera)}");

            var found = targetToSelfDirection(out Vector3 dir);
            ImGui.Text("Target to self");
            ImGui.Text($"{dir}");

            ImGui.Text("Target to self normalized");
            ImGui.Text($"{Vector3.Normalize(dir).WithY(0)}");

            ImGui.Text("Distance to target");
            ImGui.Text($"{dir.Length()}");

            ImGui.Text("Angle to target");
            ImGui.Text($"{AngleToTarget()}");

        }

        /// <summary>
        /// The X value is the west-east value (-1 -> 1)
        /// The Y value is the north-south value (-1 -> 1)
        /// </summary>
        unsafe Vector3 GetCameraLookVector(Camera* camera)
        {

            // DirH: 0 == Facing North, -PI/2 facing east, PI/2 facing west, PI= south
            var lr = -(float)Math.Sin(camera->DirH);
            var fb = -(float)Math.Cos(camera->DirH);

            return new Vector3(lr, 0, fb);
        }


        bool targetToSelfDirection(out Vector3 vector)
        {
            var target = Plugin.TargetManager.Target;

            var localPlayer = Plugin.ObjectTable.LocalPlayer;

            if (target == null || localPlayer == null)
            {
                vector = Vector3.Zero;
                return false;
            }

            Vector3 targetPosition = target.Position;
            Vector3 selfPosition = localPlayer.Position;

            Vector3 diff = targetPosition - selfPosition;

            vector = diff;
            return true;
        }

        unsafe float AngleToTarget()
        {
            var camera = GetCamera();
            bool success = targetToSelfDirection(out Vector3 diff);

            Vector2 targetVec2 = new Vector2(diff.X, diff.Z);

            Vector3 cameraLookVector = GetCameraLookVector(camera);
            Vector2 cVec2 = new Vector2(cameraLookVector.X, cameraLookVector.Z);

            return VectorMath.SignedAngle(cVec2, targetVec2);
        }

        unsafe Camera* GetCamera()
        {
            var cameraManager = CameraManager.Instance();
            if (cameraManager == null)
                return null;

            return cameraManager->GetActiveCamera();
        }

        unsafe (float, float) CameraDebug(Camera* camera)
        {
            return (camera->DirH, camera->DirV);
        }

        unsafe (float, float) CameraDirVMaxDebug(Camera* camera)
        {
            return (camera->DirVMin, camera->DirVMax);
        }

    }
}
