using System;
using System.Collections.Generic;
using UnityEngine;

public class PRDRandom
{
	private static float[] s_chanceTable = new float[101]
	{
		/* 0% */ 0f,
		/* 1% */ 0.0001611328f,
		/* 2% */ 0.0006103492f,
		/* 3% */ 0.001390287f,
		/* 4% */ 0.002450562f,
		/* 5% */ 0.003812454f,
		/* 6% */ 0.00544064f,
		/* 7% */ 0.007358261f,
		/* 8% */ 0.009589888f,
		/* 9% */ 0.01202478f,
		/* 10% */ 0.01469073f,
		/* 11% */ 0.01783203f,
		/* 12% */ 0.02094635f,
		/* 13% */ 0.02451059f,
		/* 14% */ 0.02819672f,
		/* 15% */ 0.03222107f,
		/* 16% */ 0.03644421f,
		/* 17% */ 0.04079834f,
		/* 18% */ 0.04570862f,
		/* 19% */ 0.05053855f,
		/* 20% */ 0.05546875f,
		/* 21% */ 0.0610963f,
		/* 22% */ 0.06668884f,
		/* 23% */ 0.07258219f,
		/* 24% */ 0.07850874f,
		/* 25% */ 0.08446875f,
		/* 26% */ 0.09119834f,
		/* 27% */ 0.09778266f,
		/* 28% */ 0.1045927f,
		/* 29% */ 0.111832f,
		/* 30% */ 0.1189695f,
		/* 31% */ 0.1262213f,
		/* 32% */ 0.1338988f,
		/* 33% */ 0.1418493f,
		/* 34% */ 0.1500781f,
		/* 35% */ 0.1580311f,
		/* 36% */ 0.1662726f,
		/* 37% */ 0.1752441f,
		/* 38% */ 0.1833201f,
		/* 39% */ 0.1924883f,
		/* 40% */ 0.2017426f,
		/* 41% */ 0.2110553f,
		/* 42% */ 0.220342f,
		/* 43% */ 0.2302755f,
		/* 44% */ 0.2397724f,
		/* 45% */ 0.2494584f,
		/* 46% */ 0.2596484f,
		/* 47% */ 0.27014f,
		/* 48% */ 0.280897f,
		/* 49% */ 0.29093f,
		/* 50% */ 0.3019776f,
		/* 51% */ 0.3127797f,
		/* 52% */ 0.322937f,
		/* 53% */ 0.3341516f,
		/* 54% */ 0.3480469f,
		/* 55% */ 0.360863f,
		/* 56% */ 0.3735327f,
		/* 57% */ 0.3854644f,
		/* 58% */ 0.3983658f,
		/* 59% */ 0.4104217f,
		/* 60% */ 0.4230034f,
		/* 61% */ 0.4348656f,
		/* 62% */ 0.4462058f,
		/* 63% */ 0.4577338f,
		/* 64% */ 0.4695821f,
		/* 65% */ 0.4809301f,
		/* 66% */ 0.4931479f,
		/* 67% */ 0.5079261f,
		/* 68% */ 0.5298079f,
		/* 69% */ 0.5507703f,
		/* 70% */ 0.5715531f,
		/* 71% */ 0.5903955f,
		/* 72% */ 0.611093f,
		/* 73% */ 0.6303659f,
		/* 74% */ 0.649352f,
		/* 75% */ 0.6665279f,
		/* 76% */ 0.6846761f,
		/* 77% */ 0.7016721f,
		/* 78% */ 0.7180756f,
		/* 79% */ 0.7338443f,
		/* 80% */ 0.7496008f,
		/* 81% */ 0.7645162f,
		/* 82% */ 0.7801579f,
		/* 83% */ 0.7954523f,
		/* 84% */ 0.8106738f,
		/* 85% */ 0.8250977f,
		/* 86% */ 0.8370593f,
		/* 87% */ 0.8508409f,
		/* 88% */ 0.8628125f,
		/* 89% */ 0.8759527f,
		/* 90% */ 0.8885157f,
		/* 91% */ 0.9015872f,
		/* 92% */ 0.9131664f,
		/* 93% */ 0.9247211f,
		/* 94% */ 0.9358692f,
		/* 95% */ 0.9473711f,
		/* 96% */ 0.9582562f,
		/* 97% */ 0.9691508f,
		/* 98% */ 0.9795209f,
		/* 99% */ 0.9899905f,
		/* 100% */ 1f,
	};

	//

	private float m_lastChance;

	private float m_baseC;

	private float m_currentC;

	private static PRDRandom _Instance = new PRDRandom();
	public static PRDRandom Instance { get { return _Instance; } }

	// ------------------------------------------------------------

	private bool Check(float chance)
	{
		return UnityEngine.Random.value < chance;
	}

	// ------------------------------------------------------------

	private float GetC(float chance)
	{
		var c = Mathf.RoundToInt(chance * 100.0f);

		//Debug.LogFormat("C: {0}", c);

		return s_chanceTable[c];
	}

	// ------------------------------------------------------------

	private void ResetCurrentC(float chance)
	{
		this.m_baseC = this.GetC(chance);
		this.m_currentC = this.m_baseC;
	}

	// ------------------------------------------------------------

	public bool Success(float chance)
	{
		if (chance != this.m_lastChance)
		{
			this.ResetCurrentC(chance);
		}

		this.m_lastChance = chance;

		//Debug.LogFormat("CurrentC: {0}", this.m_currentC);

		if (Check(this.m_currentC))
		{
			this.ResetCurrentC(chance);
			return true;
		}

		this.m_currentC += this.m_baseC;

		return false;
	}

	// ------------------------------------------------------------

	public static void Test()
	{
		var s = new PRDRandom();

		var realChance = 0.0f;
		var realTries = 1;
		var hitStreak = 0;
		var missStreak = 0;
		var subsequentHits = 0;
		var subsequentMisses = 0;

		for (int x = 0; x < realTries; x++)
		{
			var chance = 0.9f;
			var tries = 100000;
			var hits = 0;

			for (int i = 0; i < tries; i++)
			{
				if (s.Success(chance))
				{
					hits++;

					subsequentHits++;
					hitStreak = (subsequentHits >= hitStreak) ? subsequentHits : hitStreak;

					subsequentMisses = 0;
				}
				else
				{
					subsequentMisses++;
					missStreak = (subsequentMisses >= missStreak) ? subsequentMisses : missStreak;

					subsequentHits = 0;
				}
			}

			Debug.Log("\n\n");
			Debug.LogFormat("Chance: {0:P0}", chance);
			Debug.LogFormat("Tries: {0}", tries);
			Debug.LogFormat("Hits: {0}", hits);
			Debug.LogFormat("Misses: {0}", tries - hits);
			Debug.LogFormat("Hit Streak: {0}", hitStreak);
			Debug.LogFormat("Miss Streak: {0}", missStreak);

			realChance += (float)hits / tries;
		}

		Debug.LogFormat("Real Chance: {0:P1}", realChance / realTries);
	}
}