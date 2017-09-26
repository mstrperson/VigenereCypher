using System;
using System.IO;
using System.Collections.Generic;



namespace VigenereCypher
{
	/// <summary>
	/// Encrypte and Decrypt messages using a Vigenere Cypher
	/// </summary>
	public class VigenereCypher
	{
		public static readonly String ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		public static readonly int keyBase = 10;

		/// <summary>
		/// Crack the specified encryptedMessage given a key of the supplied length.
		/// </summary>
		/// <returns>Potential solutions to the cracked message.</returns>
		/// <param name="encryptedMessage">Encrypted message.</param>
		/// <param name="keyLength">Key length.</param>
		public static Dictionary<int[], string> Crack(String encryptedMessage, int keyLength)
		{
			Dictionary<int[], string> solutions = new Dictionary<int[], string>();
			int[] key = new int[keyLength];

			for (int i = 0; i < keyLength; i++)
			{
				key[i] = 0;
			}

			StreamReader reader = new StreamReader("dictionary.txt");
			List<String> dictionary = new List<string>();
			while (!reader.EndOfStream)
			{
				dictionary.Add(reader.ReadLine().Trim());
			}

			while (!IncrementKey(ref key))
			{
				String message = DecryptMessage(encryptedMessage, key);
				String[] words = message.Split(' ');

				bool solution = true;
				foreach (String word in words)
				{
					if (!dictionary.Contains(word.ToLower()))
					{
						solution = false;
						break;
					}
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
					solutions.Add((int[])key.Clone(), message);
				}
			}

			return solutions;
		}

		public static String EncryptMessage(String msg, int[] key)
		{
			String encrypted = "";
			for (int i = 0; i < msg.Length; i++)
			{
				encrypted += EncryptCharacter(ALPHABET, msg[i], key[i % key.Length]);
			}

			return encrypted;
		}

		public static String DecryptMessage(string msg, int[] key)
		{
			String encrypted = "";
			for (int i = 0; i<msg.Length; i++)
			{
				encrypted += DecryptCharacter(ALPHABET, msg[i], key[i % key.Length]);
			}

			return encrypted;
		}

		public static bool IncrementKey(ref int[] key)
		{
			int place = key.Length - 1;
			key[place] = (key[place] + 1) % keyBase;
			bool done = key[place] != 0;
			while (!done && place > 0)
			{
				place--;
				key[place] = (key[place] + 1) % keyBase;

				done = key[place] != 0;
			}

			return !done;
		}

		public static char EncryptCharacter(string charSet, char ch, int key)
		{
			int charNumber = charSet.IndexOf(ch);
			if (charNumber == -1) return ch;

			int encChNumber = (charNumber + key) % charSet.Length;
			while (encChNumber < 0)
			{
				encChNumber += charSet.Length;
			}

			char encChar = charSet[encChNumber];
			return encChar;
		}

		public static char DecryptCharacter(string charSet, char ch, int key)
		{
			return EncryptCharacter(charSet, ch, -key);
		}

		public static void Main(string[] args)
		{
			Console.WriteLine("Enter the encrypted message...");
			String encryptedMessage = Console.ReadLine().ToUpper();

			Crack(encryptedMessage, 4);

			Console.WriteLine("Done!");
			Console.ReadKey();
		}
	}
}
