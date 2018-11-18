using System.Net;
using System.Text;
using HQ.Common;
using HQ.Rosetta;
using HQ.Rosetta.Configuration;
using Microsoft.AspNetCore.Http;

namespace HQ.Domicile.Extensions
{
    public static class EnvelopingExtensions
    {
        public static void MaybeEnvelope(this HttpResponse response, HttpRequest request, QueryOptions options, object data, out object body)
        {
            if (UseEnvelope(request, options))
            {
                body = new
                {
                    response = data,
                    status = response.StatusCode,
                    headers = response.Headers
                };

                response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                body = data;
            }
        }

        public static void MaybeEnvelope(this HttpResponse response, HttpRequest request, QueryOptions options, IPageHeader header, out object body)
        {
            if (UseEnvelope(request, options))
            {
                body = new
                {
                    response = header,
                    status = response.StatusCode,
                    headers = response.Headers,
                    paging = new
                    {
                        header.TotalCount,
                        header.TotalPages,
                        NextPage = GetNextPage(request, header, options),
                        PreviousPage = GetPreviousPage(request, header, options),
                        LastPage = GetLastPage(request, header, options)
                    }
                };

                response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                var sb = new StringBuilder();
                if (header.HasNextPage)
                {
                    var nextPage = $"<{GetNextPage(request, header, options)}>; rel=\"next\"";
                    sb.Append(nextPage);
                }
                if (header.HasPreviousPage)
                {
                    var previousPage = $"<{GetPreviousPage(request, header, options)}>; rel=\"previous\"";
                    sb.Append(previousPage);
                }
                if (header.TotalPages > 1)
                {
                    var lastPage = $"<{GetLastPage(request, header, options)}>; rel=\"last\"";
                    sb.Append(lastPage);
                }
                if (sb.Length > 0)
                {
                    response.Headers.Add(Constants.HttpHeaders.Link, sb.ToString());
                }
                response.Headers.Add(options.TotalCountHeader, header.TotalCount.ToString());
                response.Headers.Add(options.TotalPagesHeader, header.TotalPages.ToString());
                body = header;
            }
        }

        internal static bool UseEnvelope(this HttpRequest request, QueryOptions options)
        {
            bool useEnvelope;
            if (request.Query.TryGetValue(options.EnvelopeOperator, out var envelope))
                bool.TryParse(envelope, out useEnvelope);
            else
                useEnvelope = false;
            return useEnvelope;
        }

        internal static string GetPreviousPage(HttpRequest request, IPageHeader header, QueryOptions options)
        {
            if (!header.HasPreviousPage)
                return null;
            return $"{request.Scheme}://{request.Host}{request.Path}?{options.PageOperator}={header.Index - 1}&{options.PerPageOperator}={header.Size}";
        }

        internal static string GetNextPage(HttpRequest request, IPageHeader header, QueryOptions options)
        {
            if (!header.HasNextPage)
                return null;
            return $"{request.Scheme}://{request.Host}{request.Path}?{options.PageOperator}={header.Index + 2}&{options.PerPageOperator}={header.Size}";
        }

        internal static string GetLastPage(HttpRequest request, IPageHeader header, QueryOptions options)
        {
            if (header.TotalPages < 2)
                return null;
            return $"{request.Scheme}://{request.Host}{request.Path}?{options.PageOperator}={header.TotalPages}&{options.PerPageOperator}={header.Size}";
        }
    }
}
