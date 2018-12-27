using System;
using System.Collections.Generic;
using System.Net;
using HQ.Rosetta.Configuration;
using HQ.Strings;

namespace HQ.Rosetta
{
    public class StreamOptions : IQueryValidator
    {
        public static readonly StreamOptions Empty = new StreamOptions();

        public long After { get; set; }
        public long Before { get; set; }

        public bool Validate(Type type, QueryOptions options, out IList<Error> errors)
        {
            var list = new List<Error>();
            if (After < 1)
                list.Add(new Error(ErrorEvents.InvalidPageParameter, ErrorStrings.Rosetta_PageRangeInvalid,
                    HttpStatusCode.BadRequest));
            if (Before - After > options.PerPageMax)
                list.Add(new Error(ErrorEvents.InvalidPageParameter, ErrorStrings.Rosetta_PerPageTooHigh,
                    HttpStatusCode.RequestEntityTooLarge));

            errors = list;
            return list.Count == 0;
        }
    }
}