using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Windows;

using MathNet.Numerics.LinearAlgebra;
using Interpolate = MathNet.Numerics.Interpolate;

public partial class EBM {
	public static double avg_temp;
	public static double arctic_temp;
	public static double tropics_temp;
	public static double subtropics_temp;
	public static Matrix<double> subtropics_array;
	public static Vector<double> precip_subtropics;
	public static double average_precip_subtropics;
	public static List<double> temp_list;

	public static void Reset() {
		F = 2;
		//a0 = 0.7;
		//Clear();
	}

	/// <summary> Does entire timestep integration calculation </summary>
	public static (Matrix<double>, Matrix<double>) Integrate(Vector<double> T = null, int years = 0, int timesteps = 0)
	{
		T = T ?? 7.5f + 20 * (1 - 2 * x.PointwisePower(2));
		years = years == 0 ? dur : years;
		timesteps = timesteps == 0 ? nt : timesteps;
		Matrix<double> Tfin = Matrix<double>.Build.Dense(bands, nt, 0);
		Matrix<double> Efin = Matrix<double>.Build.Dense(bands, nt, 0);
		Matrix<double> T0fin = Matrix<double>.Build.Dense(bands, nt, 0);
		Matrix<double> ASRfin = Matrix<double>.Build.Dense(bands, nt, 0);
		
		//Matrix<double> tfin = Matrix<double>.Build.Dense()np.linspace(0, 1, nt);
		Vector<double> Tg = Vector<double>.Build.DenseOfVector(T);
		Vector<double> E = Tg * cw;
		
		for (var (i, p) = (0, 0); i < years; i++)
		{
			for (int j = 0; j < timesteps; j++)
			{
				/*				if (j % (nt / 100f) == 0)
								{
									Efin.SetColumn(p, E);
									Tfin.SetColumn(p, T);
									p++;
								}*/
				Vector<double> alpha = E.PointwiseSign().PointwiseMultiply(aw).Map(x => x < 0 ? aI : x); // aw * (E > 0) + ai * (E < 0)
				Vector<double> C = alpha.PointwiseMultiply(S.Row(j)) + cg_tau * Tg - A + F; // alpha * S[i, :] + cg_tau * Tg - A
				Vector<double> T0 = C / (M - k * Lf / E);

				if (i == dur - 1)
				{
					Efin.SetColumn(j, E);// [":", i] = E;
					Tfin.SetColumn(j, T);//[":", i] = T;
					T0fin.SetColumn(j, T0);//[":", i] = T0;
					ASRfin.SetColumn(j, alpha.PointwiseMultiply(S.Row(j)));//[":", i] = alpha * S[i, ":"];
				}

				T = Sign0(GreatOrE, E) / cw + Sign0(Less, Sign0(Less, E, T0)); // E/cw*(E >= 0)+T0*(E < 0)*(T0 < 0)
				E = E + dt * (C - M * T + Fb);
				var mklfe = M - k * Lf / E;
				var signlesset0 = Sign0(Less, E, T0);

				/*# Implicit Euler on Tg
				# latent heat transport
				# n.b. this is semi-implicit, with some derivatives are
				# calculated on the previous time step*/
				var q = RH * saturation_specific_humidity(Tg, Ps);
                var rhs1 = (dt * diffop / cg) * (Lv * q / cp);
                Tg = (kappa - Matrix<double>.Build.DiagonalOfDiagonalVector(
					Sign0(Less, signlesset0, dc / mklfe) // np.diag(dc / (M - kLf / E) * (T0 < 0) * (E < 0)
				)).Solve(Tg + rhs1 + dt_tau * (
					Sign0(GreatOrE, E) / cw + (aI * S.Row(j) - A + F). // E / cw * (E >= 0) + (ai * S[i, :] - A)
					Map2((a, b) => b != 0 ? a / b : 0, // funky division
						Sign0(Less, signlesset0, mklfe)) // (M - kLf / E) * (T0 < 0) * (E < 0)
				));
			}
		}

		avg_temp = 0;
		for (int i = 12; i < 24; i++)
		{
			for (int j = 0; j < Tfin.ColumnCount; j++)
			{
				avg_temp = avg_temp + Tfin[i, j];
			}
		}

		tropics_temp = 0;
		for (int i = 12; i <= 14; i++)
		{
			for (int j = 0; j < Tfin.ColumnCount; j++)
			{
				tropics_temp = tropics_temp + Tfin[i, j];
			}
		}

		subtropics_temp = 0;
		subtropics_array = Matrix<double>.Build.Dense(bands, nt, 0);
		for (int i = 15; i <= 17; i++)
		{
			for (int j = 0; j < Tfin.ColumnCount; j++)
			{
				subtropics_temp = subtropics_temp + Tfin[i, j];
				subtropics_array[i, j] = Tfin[i, j];
			}
		}

		arctic_temp = 0;
		for (int i = 22; i < 24; i++)
		{
			for (int j = 0; j < Tfin.ColumnCount; j++)
			{
				arctic_temp = arctic_temp + Tfin[i, j];
			}
		}

		// Calculate the average temp at each latitude
		temp_list = new List<double>();
		double temp = 0;
		for (int i = 12; i < 24; i++) {
			temp = 0;
			for (int j = i; j < i + 1; j++) {
				for (int k = 0; k < Tfin.ColumnCount; k++) {
					temp = temp + Tfin[j, k];
				}			
			}
			temp = temp / Tfin.ColumnCount;
			temp_list.Add(temp);
		}

		avg_temp = avg_temp / (12 * Tfin.ColumnCount);
		arctic_temp =  arctic_temp / (2 * Tfin.ColumnCount);
		tropics_temp =  tropics_temp / (3 * Tfin.ColumnCount);
		subtropics_temp = subtropics_temp / (3 * Tfin.ColumnCount);
		return (Tfin.SubMatrix(0, Tfin.RowCount, Tfin.ColumnCount - 100, 100), Efin.SubMatrix(0, Efin.RowCount, Efin.ColumnCount - 100, 100));
	}

	/// <summary> Calculates Precipitation of regions</summary>
	/// <param name="temp"> `Vector` of final temp column means</param>
	/// <returns> `Vector` of Precipitation - Evapouration per region </returns>
	public static Vector<double> CalcPrecip(Vector<double> temp) {
		Vector<double> dT = temp - tempControl;
		Vector<double> dp_e = dT.PointwiseMultiply(np_e) * alpha;
		return np_e + dp_e;
	}

	/// <summary> Runs main integration model </summary>
	/// <param name="input"> Optional starting temp for model, will default in model </param>
	/// <param name="years"> Optional duration to run model, will default to <see cref="dur"/> </param>
	/// <param name="timesteps"> Optional number of steps per year, will default to <see cref="nt"/> </param>
	/// <example>
	/// <code>
	/// Calc(temp[]) →(temp[], energy[], precip[])
	/// </code>
	/// </example>
	/// <returns> Tuple of double arrays(temp, energy, precip)</returns>
	public static (double[], double[], double[]) Calc(IEnumerable<double> input = null, int years = 0, int timesteps = 0) {
		var(Tfin, Efin) = Integrate(input == null ? null : Vector<double>.Build.Dense(input.ToArray()), years, timesteps);
		temp = Tfin.Column(99);
		energy = Efin.Column(99);

		if (tempControl is null) InitFirstRun(Tfin);
		precip = CalcPrecip(Vector<double>.Build.DenseOfEnumerable(Tfin.FoldByRow((mean, col) => mean + col / Tfin.ColumnCount, 0d)));
		precip_subtropics = CalcPrecip(Vector<double>.Build.DenseOfEnumerable(subtropics_array.FoldByRow((mean, col) => mean + col / subtropics_array.ColumnCount, 0d)));

		double[] average_precip_subtropics_array = Condense(precip_subtropics, 1);
		average_precip_subtropics = average_precip_subtropics_array[0];
		//return (Condense(temp, regions), Condense(energy, regions), Condense(precip, regions));
		double[] regionalTemps = new double[] { tropics_temp, subtropics_temp, arctic_temp };
		
		return (regionalTemps, Condense(energy, regions), Condense(precip, regions));
	}

	/// <summary> Initialises precipidation data array and control temp </summary>
	static void InitFirstRun(Matrix<double> T100) {
		tempControl = Vector<double>.Build.DenseOfEnumerable(T100.FoldByRow((mean, col) => mean + col / T100.ColumnCount, 0d));
		// (tempControl, energyControl) = (temp, energy);
		p_e = p_e_raw.Split(',').Select(num => Double.Parse(num.Trim(new [] { '\n', ' ', '\t' }))).ToArray();
		lat_p_e = Vector<double>.Build.Dense(p_e.Length, i => i / 2d - 90).SubVector(181, 180);
		
		f = Interpolate.Common(lat_p_e, p_e.Skip(181));
		
		var tempArray = x;
		
		for (int i = 0; i < x.Count; i++)  
		{
			tempArray[i] = Math.Asin(x[i]);
		}
		
		var tempArray2 = x;
		double[] values = new double[] { 0.67085027, 1.14287837, 1.16845783, 0.69488576, -0.3846317, -0.96954969, -1.52661404, -1.83546003, -1.76328039, -1.20641116, -0.09503519, 0.50181944, 0.9568732, 2.5678029, 0.10716688, -0.91746535, -1.14415677, -0.90529723, -0.52770197, 0.05236193, 0.65670454, 0.72760175, 0.69449037, 0.34922747 };
		for (int i = 0; i < tempArray.Count; i++)
		{
			tempArray2[i] = values[i]; // (180 / Math.PI) * tempArray[i];
		}
		np_e = tempArray2;// lat; //f(np.rad2deg(np.arcsin(x))) # net precip on model grid
		
		//np_e = lat.Map(l => f.Interpolate(l)); // TODO change this value?
	}

	public static void Clear() => (temp, energy, precip) = (null, null, null);

	/// <summary> arbitrary IEnumerable slicer </summary>
	/// <param name="vec"> array-like to be cut </param>
	/// <param name="n"> number of equal cuts </param>
	/// <param name="cuts"> array of indices for arbitrary divisions </param>
	public static IEnumerable<IEnumerable<double>> Slice(IEnumerable<double> vec, int n = -1, int[] cuts = null) =>
		Func.Lambda((int m, int j) => vec.Select((x, i) =>
				new { Index = i, Value = x })
			.GroupBy(x =>
				cuts == null ?
				x.Index / (vec.Count() / m) :
				j == cuts.Length || x.Index <= cuts[j] ? j : ++j)
			.Select(x => x.Select(v => v.Value)))
		(n == -1 ? regions : n, 0);

	public static double[] Condense(IEnumerable<double> vec, int n, int[] cuts = null) => Slice(vec, n, cuts).Select(x => x.Average()).ToArray();

	// static double Average(IEnumerable<double> vec) { return x.Average(); } // template for custom averaging function

	static Predicate<(double, double)> Less = ((double, double) t) => t.Item1 < t.Item2; // condition for ↓
	static Predicate<(double, double)> GreatOrE = ((double, double) t) => t.Item1 >= t.Item2; // condition for ↓
	/// <summary> Represents python syntax of `x * (x < 0)` where `x` is a list </summary>
	public static Vector<double> Sign0(Predicate<(double, double)> op, Vector<double> vec, Vector<double> result = null) => (result ?? vec).PointwiseMultiply(vec.Map(x => op((x, 0d)) ? 1d : 0d));

	// debug printing function
	static void Print(IEnumerable<double> nums) => UnityEngine.Debug.Log(nums == null ? "null" : nums.AsString());
	static void Print(double num) => UnityEngine.Debug.Log(num);

	public static Vector<double> saturation_specific_humidity(Vector<double> temp, double press)
	{
		var es0 = 610.78;
		var t0 = 273.16;
		var Rv = 461.5;
		var Lv = 2500000.0;
		var ep = 0.622;
		temp = temp + 273.15;
		var exponent = (1 / temp - 1 / t0);
		var value = -Lv / Rv * exponent;
		var finalArray = value;
        for (int i = 0; i < value.Count; i++) 
        {
            finalArray[i] = Math.Exp(value[i]);
        }
        var es = es0 * finalArray;
		var qs = ep * es / press;
		return qs;
	}
}
