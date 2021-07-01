﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Regex = System.Text.RegularExpressions.Regex;
using RegexOptions = System.Text.RegularExpressions.RegexOptions;

using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public static class World {
	public enum Region { Arctic, City, Forest, Fire }

	public static string worldName = "";
	public static float money = 70f, publicOpinion = 70f;
	public static int turn = 1;
	public static double[] temp, energy, precip, startingTemp;
	public static float maxTempChange = 10f;
	public static double averageTemp { get => temp?.Average() ?? 0; }
	public static Dictionary<string, Dictionary<double, List<double>>> ranges; // TODO: currently unused
	public static List < (Region, Region, string) > lineToDraw = new List < (Region, Region, string) > (); // TODO: this draws lines on the overworld, store it there instead?

	public static void DetermineImpact() { // TODO: set up restart prompt or auto with player guide
		if (averageTemp > startingTemp.Average() + maxTempChange) {
			Debug.Log("hey it's hot, you might wanna restart");
			Debug.Break();
			GameManager.Restart();
		}
	}

	public struct Factor {
		public string name, verbose;
		Action<double> update;

		public Factor(string name, string fullName, Action<double> updateFunction) {
			this.name = name;
			verbose = fullName;
			update = updateFunction;
		}

		public void Update(Region scene, double delta, Region? dest = null) {
			lineToDraw.Add((scene, dest ?? Region.City, name));
			Debug.Log($"change {verbose} by {delta}");
			update.Invoke(delta);
		}
	}

	public static readonly Factor co2 = new Factor("co2", "Emissions", new Action<double>(deltaF => EBM.F += deltaF)),
		albedo = new Factor("land", "LandUse", new Action<double>(deltaa0 => EBM.a0 += deltaa0)), //was s1
		economy = new Factor("money", "Economy", new Action<double>(delta => money += (float) delta)),
		opinion = new Factor("opinion", "PublicOpinion", new Action<double>(delta => publicOpinion += (float) delta));

	/// <summary> Get factor from shorthand or full name, strings are bad so this Method is too </summary>
	public static Factor? GetFactor(string factor) {
		bool RegexCheck(string pattern) => new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(factor);
		switch (factor) {
			case var _ when RegexCheck(@"(co2|emissions)"):
				return co2;
			case var _ when RegexCheck(@"(land|albedo)"):
				return albedo;
			case var _ when RegexCheck(@"(money|economy)"):
				return economy;
			case "opinion":
				return opinion;
			default:
				return null;
		}
	}

	/// <summary> Called on game start </summary>
	public static void Init() {
		Calc();
		startingTemp = new double[temp.Length];
		temp.CopyTo(startingTemp, 0);
	}

	public static void Calc(bool useTemp = true, int years = 0, int steps = 0) {
		var timer = new Stopwatch();
		timer.Start();
		(temp, energy, precip) = EBM.Calc(useTemp ? EBM.temp : null, years, steps);
		timer.Stop();
		Debug.Log($"Average Temp: {averageTemp} with regionals: {temp.AsString()}, calculated in {timer.ElapsedMilliseconds}ms");
	}

	public static double ChangeAverageTemp(bool useTemp = true, int years = 0, int steps = 0) {
		(temp, energy, precip) = EBM.Calc(useTemp ? EBM.temp : null, years, steps);
		Debug.Log("Average temp is: " + averageTemp);
		return averageTemp; 
	}
}
