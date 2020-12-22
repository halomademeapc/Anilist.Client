using Anilist.Client.GraphQl;
using System;
using System.Collections.Generic;
using System.Net;

namespace Anilist.Client
{
    public class OtakuException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public string Content { get; set; }

        public OtakuException(HttpStatusCode statusCode, string content) : base($"Received a {statusCode} error from the API.")
        {
            this.StatusCode = statusCode;
            this.Content = content;
        }
    }

    public class GraphQlErrorException : Exception
    {
        public IEnumerable<ErrorLocation> Locations { get; private set; }

        public GraphQlErrorException(QueryError error) : base(error.Message)
        {
            Locations = error.Locations;
        }
    }
}
