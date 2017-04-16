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
	    public float minGap = 0.5f;
	    [Tooltip("Minimal distance from point to point")]
	    public float minDistance = 1f;
	    [Tooltip("Maximal distance from point to point")]
	    public float maxDistance = 5f;
	    [Tooltip("The X position of the first point")]
	    public float startingX;
        [SerializeField] public Queue<float> midPoints = new Queue<float>();
        public int midpointsNum = 10;
        public float prevAvg = 0f;
        public bool isUp = true;
        public float diffToCaveRatio = 20f;
        public int difficultyToggle = 3;


        private readonly ObjectPool<TerrainPoint> terrainPoints = new ObjectPool<TerrainPoint>(5, 5);

	    /// <summary>
	    /// Returns the next point according to the difficulty level.
	    /// </summary>
	    /// <param name="difficulty">The difficulty level. Min level is 0. There is
	    /// no limit on the max level.</param>
	    public static TerrainPoint GetNextPoint(int difficulty) {
	        return instance._GetNextPoint(difficulty);
	    }

	    private TerrainPoint _GetNextPoint(int difficulty) {
	        var point = terrainPoints.Borrow();
	        point.difficulty = difficulty;

	        // EXERCISE: Difficulty should affect distance from previous point,
	        //           height differences from previous point (slope) and gap
	        //           between floor and ceiling.
	        //           Need to take into account previous terrain to make sure the
	        //           terrain is passable: don’t create tunnels that are too
	        //           narrow or slopes that are too steep for the bat to maneuver.

	        if (difficulty == 0) {
                // Create a nice random tunnel that is very wide.
                // Note that difficulty is 0 before the game starts, so this should
                // never collide with the bat that is flying at the center of the
                // cave.
                point.floorY = Random.Range(minFloor, minFloor + 0.5f);
                point.ceilingY = Random.Range(maxCeiling - 0.5f, maxCeiling);
	        } else {
                this.setYByDifficulty(ref point, difficulty);
                //point.floorY = Random.Range(minFloor, maxCeiling - minGap);
                //point.ceilingY = Random.Range(point.floorY + minGap, maxCeiling);
            }
	        // Choose the distance from the previous point.
	        point.distanceFromPrevious = Random.Range(minDistance, maxDistance);

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

            if (isUp)
            {
                // Narrow cave up towards the ceiling using last points avg
                point.floorY = Random.Range((minFloor + (prevAvg + 0.1f) * 2f) / 3f + difficulty / diffToCaveRatio, maxCeiling - minGap);
                point.floorY = Mathf.Clamp(point.floorY, minFloor, maxCeiling - minGap);
                point.ceilingY = Random.Range(point.floorY + minGap, maxCeiling);
            }
            else {

                // Narrow cave down towards the floor using last points avg
                point.ceilingY = Random.Range(minFloor + minGap, (maxCeiling + (prevAvg - 0.1f) * 2f) / 3f - difficulty / diffToCaveRatio);
                point.ceilingY = Mathf.Clamp(point.floorY, minFloor + minGap, maxCeiling);
                point.floorY = Random.Range(minFloor, point.ceilingY - minGap);
            }

            // Calc sliding avg of cave center
            float avg = (point.floorY + point.ceilingY) / 2f;
            midPoints.Enqueue(avg);
            if (midPoints.Count() > midpointsNum)
            {
                midPoints.Dequeue();
                Debug.Log("removing");
            }
            Debug.Log(midPoints.Count().ToString());
            var allAvgs = "";
            for (int i = 0; i < midPoints.Count(); i++){
                allAvgs += (midPoints.ToArray()[i].ToString() + ", ");
            }
            Debug.Log(allAvgs);
            float slidingAvg = midPoints.Average();

            // Toggle up / down direction
            if (difficulty % difficultyToggle == 0) {
                Debug.Log("Changing Direction");
                isUp = !isUp;
                difficultyToggle = (int)Mathf.Clamp((int)difficultyToggle - 1, 1, Mathf.Infinity);
                diffToCaveRatio = Mathf.Clamp(diffToCaveRatio - 4f, 5f, Mathf.Infinity);
            }

            // Save curr avg as previous avg
            prevAvg = slidingAvg;

        }


	
	}
}
