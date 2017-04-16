using UnityEngine;
using Infra.Collections;
using System.Linq;
using System.Collections.Generic;

namespace BatCave.Terrain {
	public class TerrainGenerator : MonoSingleton<TerrainGenerator> {
	    [System.Serializable]
	    public class TerrainPoint {
	        public float x;
	        public float ceilingY;
	        public float floorY;
	        [Tooltip("Distance from previous point.\n" +
	            "Determines the chunk size from the previous point to this one.")]
	        public float distanceFromPrevious;
	        [Tooltip("Kept here for easy future reference mostly at OnPlayerPassedPoint.\n" +
	            "We might not have the points before the one passed, and this is " +
	            "needed to assess the difficulty of passing it.")]
	        public int difficulty;

	        public float GetY(bool isCeiling) {
	            return isCeiling ? ceilingY : floorY;
	        }
	    }

        public float minFloor = -1.6f;
	    public float maxCeiling = 1.6f;
	    [Tooltip("The minimal gap that could be created.\n" +
	        "Make sure this is no less than the height of the bat's collider.")]
	    public float minGap = 0.95f;
	    [Tooltip("Distance from point to point")]
	    public float pointsDistance = 5f;
        public float minPointsDistance = 1f;
        public float disanceDecayRate = 0.75f;
	    [Tooltip("The X position of the first point")]
	    public float startingX;
        [SerializeField] public Queue<float> midPoints = new Queue<float>();
        public int midpointsNum = 5;
        public float prevAvg = 0f;
        public bool isUp = true;
        public float diffToCaveRatio = 25f;
        public int caveDirectionToggle = 4;
        public int pointsCounter = 0;
        public float startTime;
        public float smoothStepTime;
        public float duration = 30f;
        public float prevFloorY;
        public float prevCeilingY;


        private readonly ObjectPool<TerrainPoint> terrainPoints = new ObjectPool<TerrainPoint>(5, 5);

	    /// <summary>
	    /// Returns the next point according to the difficulty level.
	    /// </summary>
	    /// <param name="difficulty">The difficulty level. Min level is 0. There is
	    /// no limit on the max level.</param>
	    public static TerrainPoint GetNextPoint(int difficulty) {
	        return instance._GetNextPoint(difficulty);
	    }

        void Start()
        {
            startTime = Time.time;
            prevFloorY = minFloor;
            prevCeilingY = maxCeiling;
        }

        private TerrainPoint _GetNextPoint(int difficulty) {
	        var point = terrainPoints.Borrow();
	        point.difficulty = difficulty;

	        if (difficulty == 0) {
                // Create a nice random tunnel that is very wide.
                // Note that difficulty is 0 before the game starts, so this should
                // never collide with the bat that is flying at the center of the
                // cave.
                point.floorY = Random.Range(minFloor, minFloor + 0.5f);
                point.ceilingY = Random.Range(maxCeiling - 0.5f, maxCeiling);
	        } else {
                this.setYByDifficulty(ref point, difficulty);
            }
            // Choose the distance from the previous point.
            point.distanceFromPrevious = pointsDistance + Random.Range(0f, 1f);

            // Set the point at the correct position based on the previous point's
            // position and the distance from it that we just set.
            if (Game.instance.terrainPoints.Count == 0) {
	            point.x = startingX;
	        } else {
	            float previousX = Game.instance.terrainPoints[Game.instance.terrainPoints.Count - 1].x;
	            point.x = previousX + point.distanceFromPrevious;
	        }

	        return point;
	    }

        private void setYByDifficulty(ref TerrainPoint point, int difficulty) {
            pointsCounter++; 

            smoothStepTime = (Time.time - startTime) / duration;
            if (isUp)
            {
                // Narrow cave up towards the last points avg
                float nextFloorYGoal = Mathf.Clamp(prevAvg + difficulty / diffToCaveRatio, minFloor, maxCeiling - minGap);
                point.floorY = Mathf.SmoothStep(prevFloorY, nextFloorYGoal, smoothStepTime);
                float floorDiff = Mathf.Abs(prevFloorY - point.floorY);
                point.ceilingY = Random.Range(point.floorY + minGap, maxCeiling);
            }
            else {
                // Narrow cave down towards the last points avg
                float nextCeilingYGoal = Mathf.Clamp(prevAvg - difficulty / diffToCaveRatio, minFloor + minGap, maxCeiling);
                point.ceilingY = Mathf.SmoothStep(prevAvg, nextCeilingYGoal, smoothStepTime);
                point.floorY = Random.Range(minFloor, point.ceilingY - minGap);
            }

            // Calc sliding avg of cave center
            float avg = (point.floorY + point.ceilingY) / 2f;
            midPoints.Enqueue(avg);
            if (midPoints.Count() > midpointsNum)
            {
                midPoints.Dequeue();
            }
            float slidingAvg = midPoints.Average();

            // Toggle up / down direction, make level harder and faster-changing
            if (pointsCounter % caveDirectionToggle == 0)
            {
                isUp = !isUp;
                caveDirectionToggle = (int)Mathf.Clamp((int)caveDirectionToggle - 1, 1, Mathf.Infinity);
                pointsDistance = Mathf.Max(pointsDistance * disanceDecayRate, minPointsDistance);
                diffToCaveRatio = Mathf.Clamp(diffToCaveRatio - 4f, 5f, Mathf.Infinity);
                startTime = Time.time;
            }

            // Save curr as previous
            prevAvg = slidingAvg;
            prevCeilingY = point.ceilingY;
            prevFloorY = point.floorY;

        }


	
	}
}
