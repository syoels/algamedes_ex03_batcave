using UnityEngine;
using System;

namespace BatCave.Spline {
	public class SingleSpline {
		private int n;
		private Vector2[] points;
		private float[] f_a;
		private float[] f_b;

	    public SingleSpline(Vector2[] controlPoints) {

			//TODO check size of controlPoints

			this.n = controlPoints.Length;
			// after some considerations, we decide to increase the size by one so the index of the functions 
			// and there corresponding a's and b's will start from 1 and not 0 and be the same (otherwise we 
			// would have needed to also change all the x's indexes and it would be less readable).
			f_a = new float[n+1];
			f_b = new float[n+1];
			points = new Vector2 [n + 1];
			points [0] = new Vector2(0f,0f);
			Array.Copy(controlPoints,0,points, 1, n); //in order to use it later for Value
			float[] a_vals = new float[n + 1]; //same size for index readabilty - a_vals[0] is never used
			float[] b_vals = new float[n + 1];
			float[] c_vals = new float[n];
			float[] d_vals = new float[n + 1];

			//using the first equation:
			b_vals [0] = 2f / (points [1].x - points [0].x);
			c_vals [0] = 1f / (points [1].x - points [0].x);
			d_vals [0] = 3f * (points [1].y - points [0].y) / Mathf.Pow ((points [1].x - points [0].x), 2f);

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

			//Thomas Algorithm
			float[] c_tag = new float[n];
			float[] d_tag = new float[n + 1];
			float[] k_vals = new float[n + 1];

			// calculating c_tag and d_tag
			c_tag [0] = c_vals [0] / b_vals [0];
			d_tag [0] = d_vals [0] / b_vals [0];

			for (int i = 1; i < n; i++) {
				c_tag [i] = c_vals [i] / (b_vals [i] - a_vals [i]);
				d_tag [i] = (d_vals [i] - a_vals [i] * d_tag [i - 1]) / (b_vals [i] - a_vals [i] * c_tag [i - 1]);
			}
			d_tag [n] = (d_vals [n] - a_vals [n] * d_tag [n - 1]) / (b_vals [n] - a_vals [n] * c_tag [n - 1]);

			//calculating the k_vals
			k_vals[n] = d_tag[n];
			for (int i = n - 1; i >= 0; i--) {
				k_vals [i] = d_tag [i] - c_tag [i] * k_vals [i + 1];
			}

			//caculating the f_a and f_b
			for (int i = 1; i <= n; i++) {
				f_a [i] = k_vals [i - 1] * (points [i].x - points [i - 1].x) - (points [i].y - points [i - 1].y);
				f_b [i] = -k_vals [i] * (points [i].x - points [i - 1].x) + (points [i].y - points [i - 1].y);
			}
	    }
				

	    /// <summary>
	    /// Returns the value of the spline at point X.
	    /// </summary>
	    public float Value(float X) {
	        // TODO: Implement: find the polynom f_i that passes at X and calculate
	        //       f_i(X).
			int i = this.getIByX(X);
			float t = (X - points [i - 1].x) / (points [i].x - points [i - 1].x);
			float calc = (1 - t) * points [i - 1].y +
			             t * points [i].y +
			             t * (1 - t) * (f_a [i] * (1 - t) + f_b [i] * t);
			return calc;
	    }

		private int getIByX(float x){
			if (points [0].x > x) {
				//TODO maybe throw exception, x is lower the range of points.
			}

			for (int i = 1; i <= n; i++) {
				if (points [i].x > x) {
					return i;
				}
			}
			//this means x is greater than the range of points. TODO maybe throw exception
			return -1;
		}
	}
}
