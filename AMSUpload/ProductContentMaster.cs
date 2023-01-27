using System;

namespace AMSUpload
{

	public class ProductContentMaster
	{
		public int ProductContentId { get; set; }

		public string ProductContentCode { get; set; }

		public string ProductContentName { get; set; }

		public string ProductContentDisplayName { get; set; }

		public string ProductContentDescription { get; set; }

		public string ProductCode { get; set; }

		public string VersionId { get; set; }

		public string ProductContentFileUrl { get; set; }

		public string ProductContentImageUrl { get; set; }

		public string KeyPath { get; set; }

		public string BoardCode { get; set; }

		public string CourseCode { get; set; }

		public string SubjectCode { get; set; }

		public string ChapterCode { get; set; }

		public string TopicCode { get; set; }

		public string SubTopicCode { get; set; }

		public string ModuleCode { get; set; }

		public string LessonPlanCode { get; set; }

		public string LocationCode { get; set; }

		public string ContentTypeCode { get; set; }

		public string TestCode { get; set; }

		public string Dimension1 { get; set; }

		public string Dimension1Unit { get; set; }

		public string Dimension1Value { get; set; }

		public string Dimension2 { get; set; }

		public string Dimension2Unit { get; set; }

		public string Dimension2Value { get; set; }

		public string Dimension3 { get; set; }

		public string Dimension3Unit { get; set; }

		public string Dimension3Value { get; set; }

		public string Dimension4 { get; set; }

		public string Dimension4Unit { get; set; }

		public string Dimension4Value { get; set; }

		public string Dimension5 { get; set; }

		public string Dimension5Unit { get; set; }

		public string Dimension5Value { get; set; }

		public decimal TotalContentRating { get; set; }

		public long TotalUserRatedContent { get; set; }

		public DateTime? RatingModifiedOn { get; set; }

		public DateTime CreatedOn { get; set; }

		public string CreatedBy { get; set; }

		public DateTime? ModifiedOn { get; set; }

		public string ModifiedBy { get; set; }

		public bool IsActive { get; set; }

		public bool IsDeleted { get; set; }

		public bool? isavchanged { get; set; }
	}
}