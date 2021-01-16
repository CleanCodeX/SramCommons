﻿using System;
using System.Diagnostics.CodeAnalysis;
using Common.Shared.Min.Extensions;

namespace SramCommons.Extensions
{
	public static class StringExtensions
	{
		private const int CharSize = sizeof(char);

		public static unsafe byte[] GetBytes([NotNull] string str)
		{
			str.ThrowIfNull(nameof(str));

			if (str.Length == 0) return Array.Empty<byte>();

			fixed (char* p = str)
				return new Span<byte>(p, str.Length * CharSize).ToArray();
		}

		public static unsafe string GetString([NotNull] byte[] bytes)
		{
			bytes.ThrowIfNull(nameof(bytes));

			if (bytes.Length % CharSize != 0) throw new ArgumentException($"Invalid {nameof(bytes)} length");
			if (bytes.Length == 0) return string.Empty;

			fixed (byte* p = bytes)
				return new string(new Span<char>(p, bytes.Length / CharSize));
		}
	}
}
