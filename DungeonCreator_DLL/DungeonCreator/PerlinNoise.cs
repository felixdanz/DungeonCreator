/*
 * Improved Noise (Perlin Noise)
 * 
 * C# implementation based on the java reference implementation by Key Perlin.
 * Reference implementation from: https://mrl.cs.nyu.edu/~perlin/noise/
 *
 * Methods added by me (felixdanz):
 * GenerateNoiseMap()
 * -	based on "Procedural Landmass Generation" Video Series
 *		by Sebastian Lague
 *		accompanying repository:
 *		https://github.com/SebLague/Procedural-Landmass-Generation
 * 
 */

using System;

namespace DungeonCreator
{
	public static class PerlinNoise
	{
		private static readonly int[] Permutation = 
		{
			151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 
			103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 
			26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 
			87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 
			77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 
			46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 
			187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 
			198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 
			255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 
			170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 
			172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 
			104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 
			241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 
			157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 
			93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180 
		};

		private static readonly int[] P = new int[512];
		
		
		static PerlinNoise()
		{
			for (var i = 0; i < 256; i++)
			{
				P[256 + i] = P[i] = Permutation[i];
			}
		}
		
		internal static float[] GenerateNoiseMap(
			int width, 
			int height,
			int seed,
			float scale,
			int octaves,
			float persistence,
			float lacunarity,
			(int x, int y) offset)
		{
			var rng = new Random(seed);
			var noiseMap = new float[width * height];
			var offsets = new (int x, int y)[octaves];

			for (var i = 0; i < octaves; i++)
			{
				offsets[i].x = rng.Next(-100000, 100000) + offset.x;
				offsets[i].y = rng.Next(-100000, 100000) + offset.y;
			}

			var minHeight = float.MaxValue;
			var maxHeight = float.MinValue;

			var halfWidth = width / 2.0f;
			var halfHeight = height / 2.0f;
			
			for (int y = 0, i = 0; y < height; y++)
			for (int x = 0; x < width; x++, i++)
			{
				var amplitude = 1.0f;
				var frequency = 1.0f;
				var noiseHeight = 0.0f;
				
				for (var o = 0; o < octaves; o++)
				{
					var sampleX = (x - halfWidth) / scale * frequency + offsets[o].x;
					var sampleY = (y - halfHeight) / scale * frequency + offsets[o].y;

					noiseHeight += (float) Noise(sampleX, sampleY) * amplitude;

					amplitude *= persistence;
					frequency *= lacunarity;
				}

				noiseMap[i] = noiseHeight;

				if (noiseHeight > maxHeight)
				{
					maxHeight = noiseHeight;
				}
				else if (noiseHeight < minHeight)
				{
					minHeight = noiseHeight;
				}
			}

			for (var i = 0; i < noiseMap.Length; i++)
			{
				noiseMap[i] = (noiseMap[i] - minHeight) / (maxHeight - minHeight);
			}

			return noiseMap;
		}

		private static double Noise(double x, double y, double z = 0)
		{
			// FIND UNIT CUBE THAT CONTAINS POINT.
			var X = (int) Math.Floor(x) & 255;
			var Y = (int) Math.Floor(y) & 255;
			var Z = (int) Math.Floor(z) & 255;
			
			// FIND RELATIVE X,Y,Z OF POINT IN CUBE.
			x -= Math.Floor(x);
			y -= Math.Floor(y);
			z -= Math.Floor(z);

			// COMPUTE FADE CURVES FOR EACH OF X,Y,Z.
			var u = Fade(x);
			var v = Fade(y);
			var w = Fade(z);

			// HASH COORDINATES OF THE 8 CUBE CORNERS
			var a = P[X] + Y;
			var aa = P[a] + Z;
			var ab = P[a + 1] + Z;      
			var b = P[X + 1] + Y;
			var ba = P[b] + Z;
			var bb = P[b+1]+Z;

			// AND ADD BLENDED RESULTS FROM 8 CORNERS OF CUBE
			return Lerp(
				w, 
				Lerp(
					v, 
					Lerp(
						u, 
						Grad(P[aa], x, y, z),  
						Grad(P[ba], x - 1, y, z)),
					Lerp(
						u, 
						Grad(P[ab], x, y - 1, z),
						Grad(P[bb], x - 1, y - 1, z))),
				Lerp(
					v, 
					Lerp(
						u, 
						Grad(P[aa + 1], x, y, z - 1),
						Grad(P[ba + 1], x - 1, y, z - 1)),
					Lerp(
						u, 
						Grad(P[ab + 1], x, y - 1, z - 1),
						Grad(P[bb + 1], x - 1, y - 1, z - 1))));
		}

		private static double Fade(double t)
		{
			return t * t * t * (t * (t * 6 - 15) + 10);
		}

		private static double Lerp(double t, double a, double b)
		{
			return a + t * (b - a);
		}
		
		private static double Grad(int hash, double x, double y, double z) 
		{
			// CONVERT LO 4 BITS OF HASH CODE
			var h = hash & 15;									
			
			// INTO 12 GRADIENT DIRECTIONS.
			var u = h < 8 
				? x 
				: y;
			
			var v = h < 4 
				? y 
				: h == 12 || h == 14 
					? x 
					: z;		
			
			return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
		}
	}
}