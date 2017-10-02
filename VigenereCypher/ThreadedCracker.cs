using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VigenereCypher
{
	public class ThreadedCracker : VigenereCypher
	{
		protected List<String> dictionary;
		protected DateTime StartTime;
		public TimeSpan Duration;
		protected TimeSpan PrevDuration;
		protected readonly int ThreadCount = 8;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VigenereCypher.ThreadedCracker"/> class.
		/// This object will be the Cracker that uses multiple threads.
		/// </summary>
		public ThreadedCracker()
		{
			StreamReader reader = new StreamReader("dictionary.txt");
			dictionary = new List<string>();
			while (!reader.EndOfStream)
			{
				dictionary.Add(reader.ReadLine().Trim());
			}

			// use all available processors!
			ThreadCount = Environment.ProcessorCount;
			Console.WriteLine("Initializing Cracker with {0} Processors.", ThreadCount);
			StartTime = new DateTime();
			Duration = new TimeSpan();
			PrevDuration = new TimeSpan();
		}

		/// <summary>
		/// Crack the specified encryptedMessage with the given Key length. Using the number of CPUs available.
		/// </summary>
		/// <returns>Possible solutions for the cracked message indexed by their key[].</returns>
		/// <param name="encryptedMessage">Encrypted message.</param>
		/// <param name="keyLength">Key length.</param>
		new public Dictionary<int[], String> Crack(String encryptedMessage, int keyLength)
		{
			if (keyLength <= 3) return VigenereCypher.Crack(encryptedMessage, keyLength);

			StartTime = DateTime.Now;
			Dictionary<int[], String> solutions = new Dictionary<int[], string>();


			int[] key = new int[keyLength];
			for (int i = 0; i < keyLength; i++)
			{
				key[i] = 0;
			}
			Console.WriteLine("Starting crack Process at:  {0}", StartTime.ToLongTimeString());
			PrevDuration = new TimeSpan(0);
			Duration = new TimeSpan(0);

			Task<Dictionary<int[], String>>[] Threads = new Task<Dictionary<int[], String>>[ThreadCount];

			for (int c = 0; c < ThreadCount; c++)
			{
				Threads[c] = new Task<Dictionary<int[], String>>(delegate { return CrackCheck(encryptedMessage, key); });
				Threads[c].Start();
				IncrementKey(ref key);
			}

			while (!KeyEqualsZero(key))
			{
				for (int i = 0; i < ThreadCount; i++)
				{
					if (Threads[i].IsCompleted)
					{
						MergeDictionaries(ref solutions, Threads[i].Result);
						Threads[i] = new Task<Dictionary<int[], string>>(delegate { return CrackCheck(encryptedMessage, key); });
						Threads[i].Start();
						IncrementKey(ref key);
					}
				}

				Duration = DateTime.Now - StartTime;
				if (Duration - PrevDuration > TimeSpan.FromMinutes(1))
				{
					PrevDuration = Duration;
					Console.WriteLine("{0} has elapsed.", Duration);
					Console.Write("Attempting Current Key:  ");
                    PrintKey(key);
					Console.WriteLine("___________________________________");
				}

				Thread.Sleep(20);
			}

			return solutions;
		}

		/// <summary>
		/// Merges the dictionaries.
		/// the second parameter is merged into the Target parameter.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="toMerge">To merge.</param>
		public void MergeDictionaries(ref Dictionary<int[], string> target, Dictionary<int[], string> toMerge)
		{
			foreach (int[] key in toMerge.Keys)
			{
				if (!target.ContainsKey(key))
					target.Add(key, toMerge[key]);
			}
		}

		/// <summary>
		/// Check to see if the Key is all zero.
		/// </summary>
		/// <returns><c>true</c>, if key is all zeros, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public bool KeyEqualsZero(int[] key)
		{
			foreach (int n in key)
			{
				if (n != 0) return false;
			}

			return true;
		}


		/// <summary>
		/// Check to see if the given key is a solution to the problem.
		/// </summary>
		/// <returns>The check.</returns>
		/// <param name="encryptedMessage">Encrypted message.</param>
		/// <param name="key">Key.</param>
		public Dictionary<int[], String> CrackCheck(String encryptedMessage, int[] key)
		{
			Dictionary<int[], String> mysolutions = new Dictionary<int[], string>();
			String message = DecryptMessage(encryptedMessage, key);
			String[] words = message.Split(' ');

			bool solution = true;
			int count = 0;
			foreach (String word in words)
			{
				if (!dictionary.Contains(word.ToLower()))
				{
					solution = false;
					break;
				}

				if (++count > 4 && solution)
					break;
			}

			if (solution)
			{
				Console.WriteLine("Found Solution:  {0}", message);
				Console.Write("Key:  ");
				for (int i = 0; i<key.Length; i++)
				{
					Console.Write("{0} ", key[i]);
				}
				Console.WriteLine();
				mysolutions.Add((int[])key.Clone(), message);
			}
			return mysolutions;
		}
	}
}
