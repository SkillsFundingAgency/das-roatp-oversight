using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SFA.DAS.RoatpOversight.Web.TagHelpers
{
    [HtmlTargetElement("div", Attributes = HighlightErrorForAttributeName + "," + ErrorCssClass)]
    public class DasHighlightErrorsListTagHelper : TagHelper
    {
        private const string HighlightErrorForAttributeName = "das-highlight-error-for-list";
        private const string ErrorCssClass = "error-class";

        /// <summary>
        /// This is a CSV list, you can also use it to check for a single entry within the ModelState.
        /// It is not bound to a ModelExpression so will take any value/free-text.
        /// </summary>
        [HtmlAttributeName(HighlightErrorForAttributeName)]
        public string PropertyCsvList { get; set; }

        [HtmlAttributeName(ErrorCssClass)]
        public string CssClass { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrWhiteSpace(PropertyCsvList))
            {
                foreach (var propertyName in PropertyCsvList.Split(","))
                {
                    if (ViewContext.ModelState.TryGetValue(propertyName, out var modelStateEntry) && modelStateEntry.Errors.Count > 0)
                    {
                        var tagBuilder = new TagBuilder(context.TagName);
                        tagBuilder.AddCssClass(CssClass);
                        output.MergeAttributes(tagBuilder);
                        break;
                    }
                }
            }
        }
    }
}
