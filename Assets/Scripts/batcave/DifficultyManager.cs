using UnityEngine;
using BatCave.Terrain;

namespace BatCave
{
    /// <summary>
    /// Allows calculating difficulty based on player success level.
    /// </summary>
    public class DifficultyManager : MonoSingleton<DifficultyManager>
    {

        private static int _pointsCounter = 0;
        private static int _pointsPerLevel = 7;
        private static int _currLevel = 1;
        private static int _levelChageFreq = 2;

        public override void Init()
        {
            Game.OnPlayerPassedPointEvent += OnPlayerPassedPoint;
        }

        public static int GetNextDifficulty()
        {
            // Always return 0 until the game starts.
            if (!Game.instance.HasStarted) return 0;

            return _currLevel /* + Random.Range(-1, 2) */;
        }

        private void OnPlayerPassedPoint(TerrainGenerator.TerrainPoint point)
        {
            // EXERCISE: Process player success level.
            if (++_pointsCounter > _pointsPerLevel)
            {
                _pointsCounter = 0;
                _currLevel++;
                Debug.Log("Level up: " + _currLevel.ToString());

                //every _levelChageFreq levels accelarate level changing rate
                if (_currLevel % _levelChageFreq == 0)
                {
                    _pointsPerLevel = (int)Mathf.Clamp((float)_pointsPerLevel - 1f, 3f, Mathf.Infinity);
                    Debug.Log(_pointsPerLevel.ToString() + " points per level");
                }
            }
        }
    }
}
