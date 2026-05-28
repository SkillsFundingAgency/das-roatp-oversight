using System.Collections.Generic;

namespace SFA.DAS.RoatpOversight.Domain;

public record UpdateCourseTypesRequest(IEnumerable<int> CourseTypeIds, string UserId);
