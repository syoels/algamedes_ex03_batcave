using UnityEngine;
using System;

namespace BatCave.Spline {
	public class ContinuousSpline : SingleSpline {
		float k_n_minus_1;
	    public ContinuousSpline(Vector2[] firstControlPoints) : base(firstControlPoints) {
	        // Empty on purpose.
	    }

	    /// <summary>
	    /// Continue the spline with more control points.
	    /// </summary>
	    public void AddControlPoints(params Vector2[] added_points) {
			Vector2[] new_points = new Vector2[this.points.Length + added_points.Length];
			Array.Copy (this.points, 0, new_points, 0, this.points.Length);
			Array.Copy (added_points, 0, new_points, this.points.Length, added_points.Length);
			this.points = new_points;
			Debug.Log ("cur length: " + this.points.Length.ToString ());
			this.calc_f_a_b ();
	    }

		override protected void calc_k_vals(float[] c_tag, float[] d_tag, float[] k_vals){
			//calculating the k_vals
			k_vals[n] = d_tag[n];
			for (int i = n - 1; i >= 0; i--) {
				k_vals [i] = d_tag [i] - c_tag [i] * k_vals [i + 1];
			}
			this.k_n_minus_1 = k_vals [n - 1];
		}

		override protected void prepThomasMat(float[] a_vals, float[] b_vals, float[] c_vals, float[] d_vals){
			//using the first equation:
			b_vals [0] = 1f;
			c_vals [0] = 0f;
			d_vals [0] = this.k_n_minus_1;
			//using the second equation
			for (int i = 1; i < n; i++) {
				a_vals [i] = 1f / (points [i].x - points [i - 1].x);
				b_vals [i] = 2f * (1f / (points [i].x - points [i - 1].x) + 1f / (points [i + 1].x - points [i].x));
				c_vals [i] = 1f / (points [i + 1].x - points [i].x);
				d_vals [i] = 3f * (
					(points [i].y - points [i - 1].y) / Mathf.Pow ((points [i].x - points [i - 1].x), 2f) +
					(points [i + 1].y - points [i].y) / Mathf.Pow ((points [i + 1].x - points [i].x), 2f));
			}

			//using the third equation
			a_vals[n] = 1f / (points[n].x - points[n-1].x);
			b_vals [n] = 2f / (points [n].x - points [n - 1].x);
			d_vals [n] = 3f * (points [n].y - points [n - 1].y) / Mathf.Pow ((points [n].x - points [n - 1].x), 2f);
		}


	}
}
