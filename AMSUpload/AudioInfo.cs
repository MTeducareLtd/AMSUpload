using System;
using MediaInfoLib;

namespace AMSUpload
{
	public class AudioInfo
	{
		public string Codec { get; private set; }

		public string CompressionMode { get; private set; }

		public string ChannelPositions { get; private set; }

		public TimeSpan Duration { get; private set; }

		public int Bitrate { get; private set; }

		public string BitrateMode { get; private set; }

		public int SamplingRate { get; private set; }

		public AudioInfo(MediaInfo mi)
		{
			Codec = mi.Get(StreamKind.Audio, 0, "Format");
			Duration = TimeSpan.FromMilliseconds(int.Parse(mi.Get(StreamKind.Audio, 0, "Duration")));
			Bitrate = int.Parse(mi.Get(StreamKind.Audio, 0, "BitRate"));
			BitrateMode = mi.Get(StreamKind.Audio, 0, "BitRate_Mode");
			CompressionMode = mi.Get(StreamKind.Audio, 0, "Compression_Mode");
			ChannelPositions = mi.Get(StreamKind.Audio, 0, "ChannelPositions");
			SamplingRate = int.Parse(mi.Get(StreamKind.Audio, 0, "SamplingRate"));
		}
	}
}