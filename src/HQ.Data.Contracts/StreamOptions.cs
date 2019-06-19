using System;
using System.Collections.Generic;
using System.Net;
using HQ.Data.Contracts.Configuration;

namespace HQ.Data.Contracts
{
    public class StreamOptions : IQueryValidator
    {
        public static readonly StreamOptions Empty = new StreamOptions();

        public long After { get; set; }
        public long Before { get; set; }

        public bool Validate(Type type, QueryOptions options, out IList<Error> errors)
        {
            var list = new List<Error>();
            if (After < 0)
                list.Add(new Error(ErrorEvents.InvalidParameter, ErrorStrings.PageRangeInvalid,
                    HttpStatusCode.BadRequest));
            if (Before - After > options.PerPageMax)
                list.Add(new Error(ErrorEvents.InvalidParameter, ErrorStrings.PerPageTooHigh,
                    HttpStatusCode.RequestEntityTooLarge));

            errors = list;
            return list.Count == 0;
        }
    }
}
