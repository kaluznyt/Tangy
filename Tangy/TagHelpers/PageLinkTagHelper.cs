using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Tangy.Models;

namespace Tangy.TagHelpers
{
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PageLinkTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public PageLinkTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public PagingInfo PageModel { get; set; }

        public string PageAction { get; set; }

        public bool PageClassesEnabled { get; set; }

        public string PageClass { get; set; }

        public string PageClassNormal { get; set; }

        public string PageClassSelected { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

            var divTag = new TagBuilder("div");

            for (int i = 1; i <= PageModel.TotalPages; i++)
            {
                var pageLinkTags = new TagBuilder("a");

                pageLinkTags.Attributes["href"] = urlHelper.Action(new UrlActionContext
                {
                    Action = PageAction,
                    Values = new { productPage = i }
                });

                if (PageClassesEnabled)
                {
                    pageLinkTags.AddCssClass(PageClass);

                    pageLinkTags.AddCssClass(i == PageModel.CurrentPage ? PageClassSelected : PageClassNormal);
                }

                pageLinkTags.InnerHtml.Append($"{i}");
                divTag.InnerHtml.AppendHtml(pageLinkTags);
            }

            output.Content.AppendHtml(divTag.InnerHtml);
        }
    }

}
