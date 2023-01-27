using System;
using MediaInfoLib;

namespace AMSUpload
{

	public class VideoInfo
	{
		public string Codec { get; private set; }

		public int Width { get; private set; }

		public int Heigth { get; private set; }

		public double FrameRate { get; private set; }

		public string FrameRateMode { get; private set; }

		public string ScanType { get; private set; }

		public TimeSpan Duration { get; private set; }

		public int Bitrate { get; private set; }

		public string AspectRatioMode { get; private set; }

		public double AspectRatio { get; private set; }

		public VideoInfo(MediaInfo mi)
		{
			Codec = mi.Get(StreamKind.Video, 0, "Format");
			Width = int.Parse(mi.Get(StreamKind.Video, 0, "Width"));
			Heigth = int.Parse(mi.Get(StreamKind.Video, 0, "Height"));
			Duration = TimeSpan.FromMilliseconds(int.Parse(mi.Get(StreamKind.Video, 0, "Duration")));
			Bitrate = int.Parse(mi.Get(StreamKind.Video, 0, "BitRate"));
			AspectRatioMode = mi.Get(StreamKind.Video, 0, "AspectRatio/String");
			AspectRatio = double.Parse(mi.Get(StreamKind.Video, 0, "AspectRatio"));
			FrameRate = double.Parse(mi.Get(StreamKind.Video, 0, "FrameRate"));
			FrameRateMode = mi.Get(StreamKind.Video, 0, "FrameRate_Mode");
			ScanType = mi.Get(StreamKind.Video, 0, "ScanType");
		}
	}
}