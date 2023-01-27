using System;
using MediaInfoLib;

namespace AMSUpload
{

	public static class Utility
	{
		public static string GetBitRate(string path)
		{
			MediaInfo mediaInfo = new MediaInfo();
			mediaInfo.Open("E:\\IPCC-DEMO\\1-GRP-I_ACCOUNTING\\Average_Due_Date_And_Account_Current\\Average_Due_Date_And_Account_Current\\MOD1666812-M1\\01\\01-Introdution_of_Average_Due_Date.mp4");
			VideoInfo videoInfo = new VideoInfo(mediaInfo);
			AudioInfo audioInfo = new AudioInfo(mediaInfo);
			mediaInfo.Close();
			int num = videoInfo.Bitrate + audioInfo.Bitrate;
			return Convert.ToString(num / 1000);
		}

		public static MyMediaInfo GetMediaInfo(string path)
		{
			MediaInfo mediaInfo = new MediaInfo();
			mediaInfo.Open(path);
			VideoInfo v = new VideoInfo(mediaInfo);
			AudioInfo a = new AudioInfo(mediaInfo);
			mediaInfo.Close();
			return new MyMediaInfo
			{
				A = a,
				V = v
			};
		}
	}
}