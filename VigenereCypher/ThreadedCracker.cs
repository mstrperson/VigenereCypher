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

		public ThreadedCracker()
		{
			StreamReader reader = new StreamReader("dictionary.txt");
			dictionary = new List<string>();
			while (!reader.EndOfStream)
			{
				dictionary.Add(reader.ReadLine().Trim());
			}

			StartTime = new DateTime();
			Duration = new TimeSpan();
			PrevDuration = new TimeSpan();
		}

		new public Dictionary<int[], String> Crack(String encryptedMessage, int keyLength)
		{
			if (keyLength <= 3) return VigenereCypher.Crack(encryptedMessage, keyLength);

			StartTime = DateTime.Now;
			Dictionary<int[], String> solutions = new Dictionary<int[], string>();


			int[] key = new int[keyLength];
			for (int i = 0; i<keyLength; i++)
			{
				key[i] = 0;
			}
			Console.WriteLine("Starting crack Process at:  {0}", StartTime.ToLongTimeString());
            PrevDuration = new TimeSpan(0);
			Duration = new TimeSpan(0);
			int end = 0;
			while (end == 0)
			{

				Duration = DateTime.Now - StartTime;
				if (Duration - PrevDuration > TimeSpan.FromMinutes(1))
				{
					PrevDuration = Duration;
					Console.WriteLine("{0} has elapsed.", Duration);
					Console.Write("Attempting Current Key:  ");
					for (int i = 0; i < key.Length; i++)
					{
						Console.Write("{0} ", key[i]);
					}
					Console.WriteLine("\n___________________________________");
				}

				SubCrack(encryptedMessage, key, ref solutions);

			  	for (int i = 0; i < 100; i++, end = IncrementKey(ref key) ? 0 : 1)
				{
					if (end == 1) break;
			  	}

				if (end == 1) break;

			} 

			return solutions;
		}
		                                             

		public void SubCrack(String encryptedMessage, int[] startingKey, ref Dictionary<int[], String> solutions)
		{
			Dictionary<int[], string> mysolutions = new Dictionary<int[], string>();
			int[] akey = startingKey;
			List<int[]> keys = new List<int[]>();
			for (int i = 0; i < 100; i++, IncrementKey(ref akey))
			{
				keys.Add(akey);
			}
			Parallel.ForEach(keys, (key) =>
			{
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
					for (int i = 0; i < key.Length; i++)
					{
						Console.Write("{0} ", key[i]);
					}
					Console.WriteLine();
					mysolutions.Add((int[])key.Clone(), message);
				}
			});

			foreach (int[] key in mysolutions.Keys)
			{
				if (!solutions.ContainsKey(key))
				{
					solutions.Add(key, mysolutions[key]);
				}
			}

			Console.Write("Checked:  ");
			for (int i = 0; i<akey.Length; i++)
            {
                Console.Write("{0} ", startingKey[i]);
            }
			Console.Write("to: ");
			for (int i = 0; i<akey.Length; i++)
            {
                Console.Write("{0} ", akey[i]);
            }
			Console.WriteLine();
		}
	}
}
