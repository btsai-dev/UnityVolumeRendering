using System;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace UnityVolumeRendering
{
    struct InternalTexJob : IJobParallelFor
    {
        // Jobs declare all data that will be accessed in the job
        // By declaring it as read only, multiple jobs are allowed to access the data in parallel
        [ReadOnly]
        public NativeArray<int> DatasetData;

        // By default containers are assumed to be read & write
        public NativeArray<Color> ColorData;

        // Delta time must be copied to the job since jobs generally don't have concept of a frame.
        // The main thread waits for the job same frame or next frame, but the job should do work deterministically
        // independent on when the job happens to run on the worker threads.
        public int minValue;
        public int maxRange;

        // The code actually running on the job
        public void Execute(int i)
        {
            ColorData[i] = new Color((float)(DatasetData[i] - minValue) / maxRange, 0.0f, 0.0f, 0.0f);
        }
    }
/*
    public class InternalTextureJob : ThreadedJob
    {
        public int[] InData;  // arbitary job data
        public int dimX;
        public int dimY;
        public int dimZ;
        public int minValue;
        public int maxValue;
        public Color[] OutData; // arbitary job data

        protected override void ThreadFunction()
        {
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int maxRange = maxValue - minValue;
                        int iData = x + y * dimX + z * (dimX * dimY);
                        OutData[iData] = new Color((float)(InData[iData] - minValue) / maxRange, 0.0f, 0.0f, 0.0f);
                    }
                }
            }

            // Do your threaded task. DON'T use the Unity API here
            //for (int i = 0; i < 100000000; i++)
            //{
            //    InData[i % InData.Length] += InData[(i+1) % InData.Length];
            //}
        }
    }
    */
}