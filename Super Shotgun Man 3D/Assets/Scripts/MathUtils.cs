using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    public static float GaussianRandom(float a, float b)
    {
        //get current time since application start
        TimeSpan time = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
        float time_secs = time.Milliseconds / 1000.0f;

        // take the nearest whole number modulus and plug it into the gaussian equation
        float lerp_factor = time_secs % 1.0f;
        float exponent = -0.5f*MathF.Pow((lerp_factor - 0.5f) / 0.112346f, 2.0f);
        float gaussian = (2.50666f / MathF.Sqrt(2.0f * MathF.PI)) * MathF.Pow(MathF.E, exponent);

        //based on the lerp factor's side on the bell curve, calculate a value
        float midpoint = a + (b - a) / 2.0f;
        if (lerp_factor < 0.5f)
            return Mathf.Lerp(a, midpoint, gaussian);
        else
            return Mathf.Lerp(b, midpoint, gaussian);
    }//Draws just the box at where it is currently hitting.
    public static void DrawBoxCastOnHit(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float hitInfoDistance, Color color)
    {
        origin = CastCenterOnCollision(origin, direction, hitInfoDistance);
        DrawBox(origin, halfExtents, orientation, color);
    }

    //Draws the full box from start of cast to its end distance. Can also pass in hitInfoDistance instead of full distance
    public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance, Color color)
    {
        direction.Normalize();
        Box bottomBox = new Box(origin, halfExtents, orientation);
        Box topBox = new Box(origin + (direction * distance), halfExtents, orientation);

        UnityEngine.Debug.DrawLine(bottomBox.backBottomLeft, topBox.backBottomLeft, color);
        UnityEngine.Debug.DrawLine(bottomBox.backBottomRight, topBox.backBottomRight, color);
        UnityEngine.Debug.DrawLine(bottomBox.backTopLeft, topBox.backTopLeft, color);
        UnityEngine.Debug.DrawLine(bottomBox.backTopRight, topBox.backTopRight, color);
        UnityEngine.Debug.DrawLine(bottomBox.frontTopLeft, topBox.frontTopLeft, color);
        UnityEngine.Debug.DrawLine(bottomBox.frontTopRight, topBox.frontTopRight, color);
        UnityEngine.Debug.DrawLine(bottomBox.frontBottomLeft, topBox.frontBottomLeft, color);
        UnityEngine.Debug.DrawLine(bottomBox.frontBottomRight, topBox.frontBottomRight, color);

        DrawBox(bottomBox, color);
        DrawBox(topBox, color);
    }

    public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color)
    {
        DrawBox(new Box(origin, halfExtents, orientation), color);
    }
    public static void DrawBox(Box box, Color color)
    {
        UnityEngine.Debug.DrawLine(box.frontTopLeft, box.frontTopRight, color);
        UnityEngine.Debug.DrawLine(box.frontTopRight, box.frontBottomRight, color);
        UnityEngine.Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft, color);
        UnityEngine.Debug.DrawLine(box.frontBottomLeft, box.frontTopLeft, color);

        UnityEngine.Debug.DrawLine(box.backTopLeft, box.backTopRight, color);
        UnityEngine.Debug.DrawLine(box.backTopRight, box.backBottomRight, color);
        UnityEngine.Debug.DrawLine(box.backBottomRight, box.backBottomLeft, color);
        UnityEngine.Debug.DrawLine(box.backBottomLeft, box.backTopLeft, color);

        UnityEngine.Debug.DrawLine(box.frontTopLeft, box.backTopLeft, color);
        UnityEngine.Debug.DrawLine(box.frontTopRight, box.backTopRight, color);
        UnityEngine.Debug.DrawLine(box.frontBottomRight, box.backBottomRight, color);
        UnityEngine.Debug.DrawLine(box.frontBottomLeft, box.backBottomLeft, color);
    }

    public struct Box
    {
        public Vector3 localFrontTopLeft { get; private set; }
        public Vector3 localFrontTopRight { get; private set; }
        public Vector3 localFrontBottomLeft { get; private set; }
        public Vector3 localFrontBottomRight { get; private set; }
        public Vector3 localBackTopLeft { get { return -localFrontBottomRight; } }
        public Vector3 localBackTopRight { get { return -localFrontBottomLeft; } }
        public Vector3 localBackBottomLeft { get { return -localFrontTopRight; } }
        public Vector3 localBackBottomRight { get { return -localFrontTopLeft; } }

        public Vector3 frontTopLeft { get { return localFrontTopLeft + origin; } }
        public Vector3 frontTopRight { get { return localFrontTopRight + origin; } }
        public Vector3 frontBottomLeft { get { return localFrontBottomLeft + origin; } }
        public Vector3 frontBottomRight { get { return localFrontBottomRight + origin; } }
        public Vector3 backTopLeft { get { return localBackTopLeft + origin; } }
        public Vector3 backTopRight { get { return localBackTopRight + origin; } }
        public Vector3 backBottomLeft { get { return localBackBottomLeft + origin; } }
        public Vector3 backBottomRight { get { return localBackBottomRight + origin; } }

        public Vector3 origin { get; private set; }

        public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation) : this(origin, halfExtents)
        {
            Rotate(orientation);
        }
        public Box(Vector3 origin, Vector3 halfExtents)
        {
            this.localFrontTopLeft = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
            this.localFrontTopRight = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
            this.localFrontBottomLeft = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
            this.localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);

            this.origin = origin;
        }


        public void Rotate(Quaternion orientation)
        {
            localFrontTopLeft = RotatePointAroundPivot(localFrontTopLeft, Vector3.zero, orientation);
            localFrontTopRight = RotatePointAroundPivot(localFrontTopRight, Vector3.zero, orientation);
            localFrontBottomLeft = RotatePointAroundPivot(localFrontBottomLeft, Vector3.zero, orientation);
            localFrontBottomRight = RotatePointAroundPivot(localFrontBottomRight, Vector3.zero, orientation);
        }
    }

    //This should work for all cast types
    static Vector3 CastCenterOnCollision(Vector3 origin, Vector3 direction, float hitInfoDistance)
    {
        return origin + (direction.normalized * hitInfoDistance);
    }

    static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 direction = point - pivot;
        return pivot + rotation * direction;
    }

    public static void DrawPoint(Vector4 pos, float scale, Color color, float duration = 0.0f)
    {
        var sX = pos + new Vector4(+scale, 0, 0);
        var eX = pos + new Vector4(-scale, 0, 0);
        var sY = pos + new Vector4(0, +scale, 0);
        var eY = pos + new Vector4(0, -scale, 0);
        var sZ = pos + new Vector4(0, 0, +scale);
        var eZ = pos + new Vector4(0, 0, -scale);
        UnityEngine.Debug.DrawLine(sX, eX, color, duration);
        UnityEngine.Debug.DrawLine(sY, eY, color, duration);
        UnityEngine.Debug.DrawLine(sZ, eZ, color, duration);
    }
}
