using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineLineIntersection : MonoBehaviour
{
    //Are 2 vectors parallel?
    public static bool IsParallel(Vector2 v1, Vector2 v2)
    {
        //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
        if (Vector2.Angle(v1, v2) == 0f || Vector2.Angle(v1, v2) == 180f)
        {
            return true;
        }

        return false;
    }

    //Are 2 vectors orthogonal?
    public static bool IsOrthogonal(Vector2 v1, Vector2 v2)
    {
        //2 vectors are orthogonal is the dot product is 0
        //We have to check if close to 0 because of floating numbers
        if (Mathf.Abs(Vector2.Dot(v1, v2)) < 0.000001f)
        {
            return true;
        }

        return false;
    }

    //Is a point c between 2 other points a and b?
    public static bool IsBetween(Vector2 a, Vector2 b, Vector2 c)
    {
        bool isBetween = false;

        //Entire line segment
        Vector2 ab = b - a;
        //The intersection and the first point
        Vector2 ac = c - a;

        //Need to check 2 things: 
        //1. If the vectors are pointing in the same direction = if the dot product is positive
        //2. If the length of the vector between the intersection and the first point is smaller than the entire line
        if (Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude)
        {
            isBetween = true;
        }

        return isBetween;
    }

    public static bool IsIntersectingAlternative(Vector2 a1, Vector2 b1, Vector2 a2, Vector2 b2)
    {
        bool isIntersecting = false;

        //3d -> 2d
        Vector2 p1 = new Vector2(a1.x, a1.y);
        Vector2 p2 = new Vector2(a2.x, a2.y);

        Vector2 p3 = new Vector2(b1.x, b1.y);
        Vector2 p4 = new Vector2(b2.x, b2.y);

        float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

        //Make sure the denominator is > 0, if so the lines are parallel
        if (denominator != 0)
        {
            float u_a = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
            float u_b = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

            //Is intersecting if u_a and u_b are between 0 and 1
            if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
            {
                isIntersecting = true;
            }
        }

        return isIntersecting;
    }

    public static bool AreLinesIntersecting(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2, bool shouldIncludeEndPoints)
    {
        //To avoid floating point precision issues we can add a small value
        float epsilon = 0.00001f;

        bool isIntersecting = false;

        float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

        //Make sure the denominator is > 0, if not the lines are parallel
        if (denominator != 0f)
        {
            float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
            float u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

            //Are the line segments intersecting if the end points are the same
            if (shouldIncludeEndPoints)
            {
                //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
                if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon)
                {
                    isIntersecting = true;
                }
            }
            else
            {
                //Is intersecting if u_a and u_b are between 0 and 1
                if (u_a > 0f + epsilon && u_a < 1f - epsilon && u_b > 0f + epsilon && u_b < 1f - epsilon)
                {
                    isIntersecting = true;
                }
            }
        }

        return isIntersecting;
    }

    //Line segment-line segment intersection in 2d space by using the dot product
    //p1 and p2 belongs to line 1, and p3 and p4 belongs to line 2 
    public static bool AreLineSegmentsIntersectingDotProduct(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        bool isIntersecting = false;

        if (IsPointsOnDifferentSides(p1, p2, p3, p4) && IsPointsOnDifferentSides(p3, p4, p1, p2))
        {
            isIntersecting = true;
        }

        return isIntersecting;
    }

    //Are the points on different sides of a line?
    public static bool IsPointsOnDifferentSides(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        bool isOnDifferentSides = false;

        //The direction of the line
        Vector3 lineDir = p2 - p1;

        //The normal to a line is just flipping x and z and making z negative
        Vector3 lineNormal = new Vector3(-lineDir.z, lineDir.y, lineDir.x);

        //Now we need to take the dot product between the normal and the points on the other line
        float dot1 = Vector3.Dot(lineNormal, p3 - p1);
        float dot2 = Vector3.Dot(lineNormal, p4 - p1);

        //If you multiply them and get a negative value then p3 and p4 are on different sides of the line
        if (dot1 * dot2 < 0f)
        {
            isOnDifferentSides = true;
        }

        return isOnDifferentSides;
    }

    //Whats the coordinate of an intersection point between two lines in 2d space if we know they are intersecting
    //http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
    public static Vector2 GetLineLineIntersectionPoint(Vector2 l1_p1, Vector2 l1_p2, Vector2 l2_p1, Vector2 l2_p2)
    {
        float denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

        float u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

        Vector2 intersectionPoint = l1_p1 + u_a * (l1_p2 - l1_p1);

        return intersectionPoint;
    }
}
