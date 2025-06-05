using System.Collections.Generic;

namespace Midas.Presentation.Audio
{
	internal static class SharedSounds
	{
		internal sealed class Token
		{
			public Token(SoundId soundId, ISound sound)
			{
				Sound = sound;
				SoundId = soundId;
			}

			public SoundId SoundId { get; }
			public ISound Sound { get; }
		}

		private static readonly Dictionary<SoundId, List<Token>> sounds = new Dictionary<SoundId, List<Token>>();

		public static Token Acquire(SoundId soundId)
		{
			if (!sounds.TryGetValue(soundId, out var tokenList))
			{
				tokenList = new List<Token>();
				sounds[soundId] = tokenList;
			}

			var sound = tokenList.Count == 0 ? AudioService.CreateSound(soundId) : tokenList[0].Sound;
			if (sound == null)
			{
				Log.Instance.Error($"Sound '{soundId}' not found");
			}

			var result = new Token(soundId, sound);
			tokenList.Add(result);
			return result;
		}

		public static void Release(Token token)
		{
			if (!sounds.TryGetValue(token.SoundId, out var tokenList)
				|| !tokenList.Contains(token))
			{
				Log.Instance.Error($"Token {token.SoundId} was not acquired");
				return;
			}

			tokenList.Remove(token);
			if (tokenList.Count == 0)
			{
				AudioService.DestroySound(token.Sound);
			}
		}
	}
}