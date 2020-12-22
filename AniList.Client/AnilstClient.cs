using Anilist.Client.GraphQl;
using Anilist.Client.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Anilist.Client
{
    public class AnilistClient
    {
        private readonly HttpClient client;
        private readonly Uri endpointUri;

        public AnilistClient(HttpClient client, string endpointUri = "https://graphql.anilist.co")
        {
            if (!string.IsNullOrEmpty(endpointUri))
            {
                if (Uri.TryCreate(endpointUri, UriKind.Absolute, out var uri))
                {
                    client.BaseAddress = uri;
                    this.endpointUri = uri;
                }
                else
                    throw new ArgumentException($"{endpointUri} is not a valid Uri.");
            }
            this.client = client;
        }

        public async Task<Page<Media>> SearchMediaAsync(
            string name, int page = 0, MediaType? type = null, MediaFormat? format = null)
        {
            var queryBuilder = new QueryQueryBuilder()
                .WithPage(
                    new PageQueryBuilder()
                    .WithMedia(
                        new MediaQueryBuilder()
                        .WithId()
                        .WithGenres()
                        .WithType()
                        .WithTitle(
                            new MediaTitleQueryBuilder()
                                .WithNative()
                                .WithEnglish()
                                .WithRomaji()
                            )
                        .WithStartDate(new FuzzyDateQueryBuilder().WithAllScalarFields())
                        .WithEndDate(new FuzzyDateQueryBuilder().WithAllScalarFields())
                        .WithStudios(
                            new StudioConnectionQueryBuilder()
                                .WithEdges(
                                    new StudioEdgeQueryBuilder()
                                        .WithAllScalarFields()
                                        .WithNode(
                                            new StudioQueryBuilder()
                                                .WithAllScalarFields()
                                        )
                                )
                            ),
                        search: name,
                        type: type,
                        format: format
                    )
                    .WithPageInfo(
                        new PageInfoQueryBuilder()
                            .WithAllScalarFields()
                    ),
                    page: page
                );
            return (await ExecuteQuery<Page>(queryBuilder)).ToPage(p => p.Media);
        }

        public async Task<TResult> ExecuteQuery<TResult>(IGraphQlQueryBuilder queryBuilder, Dictionary<string, object> variables = null)
        {
            var requestContent = new GraphQlRequest
            {
                Query = queryBuilder.Build(),
                Variables = variables
            };
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var payload = JsonConvert.SerializeObject(requestContent, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                NullValueHandling = NullValueHandling.Ignore
            });
            using var request = new HttpRequestMessage(HttpMethod.Post, endpointUri)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            using var res = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var @string = await res.Content.ReadAsStringAsync();
            var result = Deserialize<GraphQlResponse<TResult>>(await res.Content.ReadAsStreamAsync());
            if (result.Errors?.Any() ?? false)
            {
                if (result.Errors.Count() == 1)
                    throw new GraphQlErrorException(result.Errors.FirstOrDefault());
                else
                    throw new AggregateException(result.Errors.Select(e => new GraphQlErrorException(e)));
            }
            return result.Result;
        }

        private static TResult Deserialize<TResult>(Stream stream)
        {
            if (stream == default || !stream.CanRead)
                return default;

            using var sr = new StreamReader(stream);
            using var jtr = new JsonTextReader(sr);
            var js = new JsonSerializer();
            return js.Deserialize<TResult>(jtr);
        }

        private struct GraphQlRequest
        {
            public string Query;
            public Dictionary<string, object> Variables;
        }

        private class GraphQlResponse<TResult> : GraphQl.GraphQlResponse<Dictionary<string, TResult>>
        {
            public TResult Result => Data[typeof(TResult).Name];
        }
    }
}
