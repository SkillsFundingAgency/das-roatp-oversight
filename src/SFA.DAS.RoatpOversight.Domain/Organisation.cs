using System;
using System.Collections.Generic;

namespace SFA.DAS.RoatpOversight.Domain;

public class Organisation
{
    public Guid OrganisationId { get; set; }

    public IEnumerable<AllowedCourseType> AllowedCourseTypes { get; set; } = [];
}

public record AllowedCourseType(int CourseTypeId, string CourseTypeName);
