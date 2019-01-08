using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;


public abstract class BVEntity
{
    public abstract bool Hit(Ray ray, float tmin, float tmax, out HitRecord rec);

    public Bounds AABB;
}

public class BVSphere : BVEntity
{
    public BVSphere(RayTracingSphere sphere)
    {
        Sphere = sphere;
        AABB.center = sphere.Center;
        float r = sphere.Radius;
        AABB.extents = new Vector3(r, r, r);
    }

    public override bool Hit(Ray ray, float tmin, float tmax, out HitRecord rec)
    {
        rec = Sphere.Raycast(ray);
        return rec.t > 0;
    }

    RayTracingSphere Sphere;
}


public class BVHNode : BVEntity
{
    public BVHNode(BVEntity left, BVEntity right)
    {
        mLeft = left;
        mRight = right;
        var min_corner = Vector3.Min(left.AABB.min, right.AABB.min);
        var max_corner = Vector3.Max(left.AABB.max, right.AABB.max);
        AABB.center = (min_corner + max_corner) / 2;
        AABB.extents = AABB.center - min_corner;
    }

    public override bool Hit(Ray ray, float tmin, float tmax, out HitRecord rec)
    {
        float distance;
        if(AABB.IntersectRay(ray, out distance))
        {
            if(distance >= tmin && distance <= tmax)
            {
                HitRecord left, right;
                bool hit_left = mLeft.Hit(ray, tmin, tmax, out left);
                bool hit_right = mRight.Hit(ray, tmin, tmax, out right);
                if(hit_left && hit_right)
                {
                    if(left.t < right.t)
                    {
                        rec = left;
                    }
                    else
                    {
                        rec = right;
                    }
                    return true;
                }
                else if(hit_left)
                {
                    rec = left;
                    return true;
                }
                else if(hit_right)
                {
                    rec = right;
                    return true;
                }
            }
        }
        rec = new HitRecord();
        return false;
    }

    BVEntity mLeft;
    BVEntity mRight;

    class TAxisCmp : IComparer<BVEntity>
    {
        int mIndex = 0;
        public TAxisCmp(int index)
        {
            mIndex = index;
        }

        public int Compare(BVEntity e0, BVEntity e1)
        {
            float d0 = e0.AABB.center[mIndex];
            float d1 = e1.AABB.center[mIndex];
            if (d0 < d1)
            {
                return -1;
            }
            else if (d0 > d1)
            {
                return 1;
            }
            else return 0;
        }
    }
    static IComparer<BVEntity> CmpX = new TAxisCmp(0);
    static IComparer<BVEntity> CmpY = new TAxisCmp(1);
    static IComparer<BVEntity> CmpZ = new TAxisCmp(2);

    public static BVHNode Build(List<BVEntity> entities, int start, int count)
    {
        if (count < 1)
        {
            throw new System.Exception();
        }
        if (count == 1)
        {
            var entity = entities[start];
            return new BVHNode(entity, entity);
        }
        int axis = Random.Range(0, 3);
        switch (axis)
        {
            case 0:
                entities.Sort(start, count, CmpX);
                break;
            case 1:
                entities.Sort(start, count, CmpY);
                break;
            default:
                entities.Sort(start, count, CmpZ);
                break;
        }
        if (count == 2)
        {
            return new BVHNode(entities[start], entities[start + 1]);
        }

        int count1 = count / 2;
        var left = Build(entities, start, count1);
        var right = Build(entities, start + count1, count - count1);
        return new BVHNode(left, right);
    }
}

public class RayTracingScene
{

}
