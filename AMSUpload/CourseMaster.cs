using System;

namespace AMSUpload
{
	public class CourseMaster
	{
		public int CourseId { get; set; }

		public string CourseCategoryCode { get; set; }

		public string BoardCode { get; set; }

		public string MediumCode { get; set; }

		public string DivisionCode { get; set; }

		public string CourseCode { get; set; }

		public string CourseName { get; set; }

		public string CourseDisplayName { get; set; }

		public string CourseShortName { get; set; }

		public string CourseDescription { get; set; }

		public int CourseSequenceNo { get; set; }

		public string CourseHierarchyCode { get; set; }

		public bool? Is_Online { get; set; }

		public int? FreeDuration { get; set; }

		public int? FreeVideo { get; set; }

		public int? FreeTest { get; set; }

		public string Version { get; set; }

		public string Reference_Course { get; set; }

		public DateTime CreatedOn { get; set; }

		public string CreatedBy { get; set; }

		public DateTime? ModifiedOn { get; set; }

		public string ModifiedBy { get; set; }

		public bool IsActive { get; set; }

		public bool IsDeleted { get; set; }
	}
}