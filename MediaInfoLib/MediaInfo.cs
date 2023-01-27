using System;
using System.Runtime.InteropServices;

namespace MediaInfoLib
{
	public class MediaInfo
	{
		private IntPtr Handle;

		private bool MustUseAnsi;

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfo_New();

		[DllImport("MediaInfo.dll")]
		private static extern void MediaInfo_Delete(IntPtr Handle);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfo_Open(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string FileName);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfoA_Open(IntPtr Handle, IntPtr FileName);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfo_Open_Buffer_Init(IntPtr Handle, long File_Size, long File_Offset);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfoA_Open(IntPtr Handle, long File_Size, long File_Offset);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfo_Open_Buffer_Continue(IntPtr Handle, IntPtr Buffer, IntPtr Buffer_Size);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfoA_Open_Buffer_Continue(IntPtr Handle, long File_Size, byte[] Buffer, IntPtr Buffer_Size);

		[DllImport("MediaInfo.dll")]
		private static extern long MediaInfo_Open_Buffer_Continue_GoTo_Get(IntPtr Handle);

		[DllImport("MediaInfo.dll")]
		private static extern long MediaInfoA_Open_Buffer_Continue_GoTo_Get(IntPtr Handle);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfo_Open_Buffer_Finalize(IntPtr Handle);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfoA_Open_Buffer_Finalize(IntPtr Handle);

		[DllImport("MediaInfo.dll")]
		private static extern void MediaInfo_Close(IntPtr Handle);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfo_Inform(IntPtr Handle, IntPtr Reserved);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfoA_Inform(IntPtr Handle, IntPtr Reserved);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfo_GetI(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfoA_GetI(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfo_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, [MarshalAs(UnmanagedType.LPWStr)] string Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfoA_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber, IntPtr Parameter, IntPtr KindOfInfo, IntPtr KindOfSearch);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfo_Option(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Option, [MarshalAs(UnmanagedType.LPWStr)] string Value);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfoA_Option(IntPtr Handle, IntPtr Option, IntPtr Value);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfo_State_Get(IntPtr Handle);

		[DllImport("MediaInfo.dll")]
		private static extern IntPtr MediaInfo_Count_Get(IntPtr Handle, IntPtr StreamKind, IntPtr StreamNumber);

		public MediaInfo()
		{
			try
			{
				Handle = MediaInfo_New();
			}
			catch
			{
				Handle = (IntPtr)0;
			}
			if (Environment.OSVersion.ToString().IndexOf("Windows") == -1)
			{
				MustUseAnsi = true;
			}
			else
			{
				MustUseAnsi = false;
			}
		}

		~MediaInfo()
		{
			if (!(Handle == (IntPtr)0))
			{
				MediaInfo_Delete(Handle);
			}
		}

		public int Open(string FileName)
		{
			if (Handle == (IntPtr)0)
			{
				return 0;
			}
			if (MustUseAnsi)
			{
				IntPtr intPtr = Marshal.StringToHGlobalAnsi(FileName);
				int result = (int)MediaInfoA_Open(Handle, intPtr);
				Marshal.FreeHGlobal(intPtr);
				return result;
			}
			return (int)MediaInfo_Open(Handle, FileName);
		}

		public int Open_Buffer_Init(long File_Size, long File_Offset)
		{
			if (Handle == (IntPtr)0)
			{
				return 0;
			}
			return (int)MediaInfo_Open_Buffer_Init(Handle, File_Size, File_Offset);
		}

		public int Open_Buffer_Continue(IntPtr Buffer, IntPtr Buffer_Size)
		{
			if (Handle == (IntPtr)0)
			{
				return 0;
			}
			return (int)MediaInfo_Open_Buffer_Continue(Handle, Buffer, Buffer_Size);
		}

		public long Open_Buffer_Continue_GoTo_Get()
		{
			if (Handle == (IntPtr)0)
			{
				return 0L;
			}
			return MediaInfo_Open_Buffer_Continue_GoTo_Get(Handle);
		}

		public int Open_Buffer_Finalize()
		{
			if (Handle == (IntPtr)0)
			{
				return 0;
			}
			return (int)MediaInfo_Open_Buffer_Finalize(Handle);
		}

		public void Close()
		{
			if (!(Handle == (IntPtr)0))
			{
				MediaInfo_Close(Handle);
			}
		}

		public string Inform()
		{
			if (Handle == (IntPtr)0)
			{
				return "Unable to load MediaInfo library";
			}
			if (MustUseAnsi)
			{
				return Marshal.PtrToStringAnsi(MediaInfoA_Inform(Handle, (IntPtr)0));
			}
			return Marshal.PtrToStringUni(MediaInfo_Inform(Handle, (IntPtr)0));
		}

		public string Get(StreamKind StreamKind, int StreamNumber, string Parameter, InfoKind KindOfInfo, InfoKind KindOfSearch)
		{
			if (Handle == (IntPtr)0)
			{
				return "Unable to load MediaInfo library";
			}
			if (MustUseAnsi)
			{
				IntPtr intPtr = Marshal.StringToHGlobalAnsi(Parameter);
				string result = Marshal.PtrToStringAnsi(MediaInfoA_Get(Handle, (IntPtr)(int)StreamKind, (IntPtr)StreamNumber, intPtr, (IntPtr)(int)KindOfInfo, (IntPtr)(int)KindOfSearch));
				Marshal.FreeHGlobal(intPtr);
				return result;
			}
			return Marshal.PtrToStringUni(MediaInfo_Get(Handle, (IntPtr)(int)StreamKind, (IntPtr)StreamNumber, Parameter, (IntPtr)(int)KindOfInfo, (IntPtr)(int)KindOfSearch));
		}

		public string Get(StreamKind StreamKind, int StreamNumber, int Parameter, InfoKind KindOfInfo)
		{
			if (Handle == (IntPtr)0)
			{
				return "Unable to load MediaInfo library";
			}
			if (MustUseAnsi)
			{
				return Marshal.PtrToStringAnsi(MediaInfoA_GetI(Handle, (IntPtr)(int)StreamKind, (IntPtr)StreamNumber, (IntPtr)Parameter, (IntPtr)(int)KindOfInfo));
			}
			return Marshal.PtrToStringUni(MediaInfo_GetI(Handle, (IntPtr)(int)StreamKind, (IntPtr)StreamNumber, (IntPtr)Parameter, (IntPtr)(int)KindOfInfo));
		}

		public string Option(string Option, string Value)
		{
			if (Handle == (IntPtr)0)
			{
				return "Unable to load MediaInfo library";
			}
			if (MustUseAnsi)
			{
				IntPtr intPtr = Marshal.StringToHGlobalAnsi(Option);
				IntPtr intPtr2 = Marshal.StringToHGlobalAnsi(Value);
				string result = Marshal.PtrToStringAnsi(MediaInfoA_Option(Handle, intPtr, intPtr2));
				Marshal.FreeHGlobal(intPtr);
				Marshal.FreeHGlobal(intPtr2);
				return result;
			}
			return Marshal.PtrToStringUni(MediaInfo_Option(Handle, Option, Value));
		}

		public int State_Get()
		{
			if (Handle == (IntPtr)0)
			{
				return 0;
			}
			return (int)MediaInfo_State_Get(Handle);
		}

		public int Count_Get(StreamKind StreamKind, int StreamNumber)
		{
			if (Handle == (IntPtr)0)
			{
				return 0;
			}
			return (int)MediaInfo_Count_Get(Handle, (IntPtr)(int)StreamKind, (IntPtr)StreamNumber);
		}

		public string Get(StreamKind StreamKind, int StreamNumber, string Parameter, InfoKind KindOfInfo)
		{
			return Get(StreamKind, StreamNumber, Parameter, KindOfInfo, InfoKind.Name);
		}

		public string Get(StreamKind StreamKind, int StreamNumber, string Parameter)
		{
			return Get(StreamKind, StreamNumber, Parameter, InfoKind.Text, InfoKind.Name);
		}

		public string Get(StreamKind StreamKind, int StreamNumber, int Parameter)
		{
			return Get(StreamKind, StreamNumber, Parameter, InfoKind.Text);
		}

		public string Option(string Option_)
		{
			return Option(Option_, "");
		}

		public int Count_Get(StreamKind StreamKind)
		{
			return Count_Get(StreamKind, -1);
		}
	}
}
