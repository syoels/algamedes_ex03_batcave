using UnityEngine;
using BatCave.Terrain;

namespace BatCave
{
    /// <summary>
    /// Allows calculating difficulty based on player success level.
    /// </summary>
    public class DifficultyManager : MonoSingleton<DifficultyManager>
    {

        public static int pointsCounter = 0;
        public static int pointsPerLevel = 7;
        public static int currLevel = 1;
        public static int levelChageFreq = 5;

        public override void Init()
        {
            Game.OnPlayerPassedPointEvent += OnPlayerPassedPoint;
        }

        public static int GetNextDifficulty()
        {
            // Always return 0 until the game starts.
            if (!Game.instance.HasStarted) return 0;

            return currLevel + (int)Random.Range(-1, 2);
        }

        private void OnPlayerPassedPoint(TerrainGenerator.TerrainPoint point)
        {
            // level up
            if (++pointsCounter > pointsPerLevel)
            {
                pointsCounter = 0;
                currLevel++;

                //every levelChageFreq levels accelarate level changing rate
                if (currLevel % levelChageFreq == 0)
                {
                    pointsPerLevel = (int)Mathf.Clamp((float)pointsPerLevel - 1f, 3f, Mathf.Infinity);
                }
            }
        }
    }
}
