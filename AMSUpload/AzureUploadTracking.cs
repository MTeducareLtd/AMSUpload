using System;

namespace AMSUpload
{
	public class AzureUploadTracking
	{
		public string ProductContentCode { get; set; }

		public string CourseCode { get; set; }

		public string FileUrl { get; set; }

		public bool? Mp4Uploaded { get; set; }

		public bool? EncodeStarted { get; set; }

		public bool? EncodeCompleted { get; set; }

		public bool? EncSetupCompleted { get; set; }

		public bool? IsDeleted { get; set; }

		public DateTime? UploadedOn { get; set; }

		public DateTime? EncodedOn { get; set; }

		public DateTime? EncryptedOn { get; set; }
	}
}
