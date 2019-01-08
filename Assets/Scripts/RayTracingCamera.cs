using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class RayTracingCamera : MonoBehaviour
{
    public int Width = 512;
    public int Height = 512;
    public int AntiAliasRayCount = 4;
    public int ScatterRayCount = 4;
    public int DebugX = 0;
    public int DebugY = 0;
    public bool GammarCorrection = true;
    public MeshRenderer Preview;
    public Texture2D OutputTexture;
    public Texture2D DepthTexture;

    private PixelDebugData _Debug;
    
    private RayTracingParallelJob _CurrentJob;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 400, 26), "UpdateTexture"))
        {
            _CurrentJob = CreateJob();

            int size = Width * Height;
            var handle = _CurrentJob.Schedule(Width * Height, Width);
            JobHandle.ScheduleBatchedJobs();
            handle.Complete();
            //_CurrentJob.Run(size);

            Color32[] pixels = _CurrentJob.OutputColors.ToArray();
            if (DebugX > 0 && DebugY > 0 && DebugX < _CurrentJob.Width && DebugY < _CurrentJob.Height)
            {
                int x = DebugX;
                int y = DebugY;
                //for (int y = 0; y < Height; ++y)
                //{
                //    for (int x = 0; x < Width; ++x)
                //    {
                _Debug = new PixelDebugData();
                long seed = _CurrentJob.RandomSeeds[y * Width + x];
                int maxDepth2 = 0;
                _CurrentJob.CalcPixelColor(ref seed, ref maxDepth2, x, y, _Debug);
                pixels[y * _CurrentJob.Width + x] = Color.red;
                //if (_CurrentJob.depthOverflow)
                //{
                //    x = Width;
                //    y = Height;
                //}
                //    }
                //}
            }

            if (OutputTexture)
            {
                if (OutputTexture.width != _CurrentJob.Width || OutputTexture.height != _CurrentJob.Height)
                {
                    Object.Destroy(OutputTexture);
                }
            }
            if (!OutputTexture)
            {
                OutputTexture = new Texture2D(_CurrentJob.Width, _CurrentJob.Height);

                if (Preview != null)
                {
                    Preview.material.mainTexture = OutputTexture;
                }
            }
            OutputTexture.SetPixels32(pixels);
            OutputTexture.Apply();

            if (DepthTexture)
            {
                if (DepthTexture.width != _CurrentJob.Width || DepthTexture.height != _CurrentJob.Height)
                {
                    Object.Destroy(DepthTexture);
                }
            }
            if (!DepthTexture)
            {
                DepthTexture = new Texture2D(_CurrentJob.Width, _CurrentJob.Height);
            }
            Color32[] depthPixels = new Color32[size];
            int maxDepth = 0;
            for (int i = 0; i < size; ++i)
            {
                int depth = _CurrentJob.MaxDepth[i] * 13;
                maxDepth = maxDepth > depth ? maxDepth : depth;
                depthPixels[i] = new Color32((byte)((depth >> 0) & 0xFF), (byte)((depth >> 8) & 0xFF), (byte)((depth >> 16) & 0xFF), 255);
            }
            DepthTexture.SetPixels32(depthPixels);
            DepthTexture.Apply();

            _CurrentJob.Dispose();
            Debug.Log("Max Depth: " + maxDepth);
        }
        //GUI.Label(new Rect(10, 10, 400, 26), "Current Iteration: " + _Iteration);
        if (_Debug != null)
        {
            GUI.TextArea(new Rect(10, 40, 400, 360), _Debug.DebugText);
        }
    }

    private void OnDrawGizmos()
    {
        if (_Debug != null)
        {
            var _DebugPoints = _Debug.Points;
            Gizmos.color = Color.red;
            for (int i = 1; i < _DebugPoints.Count; ++i)
            {
                Gizmos.DrawLine(_DebugPoints[i - 1], _DebugPoints[i]);
            }
        }
    }

    RayTracingParallelJob CreateJob()
    {
        RayTracingParallelJob job = new RayTracingParallelJob();

        var cam = this.GetComponent<Camera>();

        int w = Width;
        int h = Height;

        job.Width = w;
        job.Height = h;
        job.AntiAliasRayCount = AntiAliasRayCount;
        job.ScatterRayCount = ScatterRayCount;

        var offx = Mathf.Tan((float)(cam.fieldOfView * 0.5 / 180 * Mathf.PI));
        var offy = offx * w / h;
        
        job.StartX = -offx;
        job.StartY = -offy;
        job.StepX = 2 * offx / w;
        job.StepY = 2 * offy / h;

        job.Origin = cam.transform.position;
        job.CameraLocal2World = cam.transform.localToWorldMatrix;
        job.GammarCorrection = GammarCorrection;

        {
            List<SdSphere> spheres = new List<SdSphere>(Object.FindObjectsOfType<SdSphere>());
            spheres.RemoveAll(sphere => !sphere.gameObject.activeInHierarchy);
            job.SceneSpheres = new NativeArray<RayTracingSphere>(spheres.Count, Allocator.TempJob);
            for (int i = 0; i < spheres.Count; ++i)
            {
                job.SceneSpheres[i] = spheres[i].CreateRTGeometry();
            }
        }
        var size = w * h;
        {
            job.RandomSeeds = new NativeArray<long>(size, Allocator.TempJob);
            for (int i = 0; i < size; ++i)
            {
                job.RandomSeeds[i] = (long)(UnityEngine.Random.value * 0xFFFFFFFF);
            }
        }

        job.OutputColors = new NativeArray<Color32>(size, Allocator.Persistent);
        job.MaxDepth = new NativeArray<int>(size, Allocator.Persistent);

        return job;
    }
}
