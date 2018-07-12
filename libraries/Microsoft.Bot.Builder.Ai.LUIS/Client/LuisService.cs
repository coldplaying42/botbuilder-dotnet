﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS.Models;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Object that contains all the possible parameters to build Luis request.
    /// </summary>
    public sealed class LuisRequest : ILuisOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRequest"/> class.
        /// </summary>
        public LuisRequest()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisRequest"/> class.
        /// </summary>
        /// <param name="query"> The text query.</param>
        public LuisRequest(string query)
        {
            this.Query = query;
            this.Log = true;
        }

        /// <summary>
        /// Gets or sets the text query.
        /// </summary>
        /// <value>
        /// The text query.
        /// </value>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets if logging of queries to LUIS is allowed.
        /// </summary>
        /// <value>
        /// Indicates if logging of queries to LUIS is allowed.
        /// </value>
        public bool? Log { get; set; }

        /// <summary>
        /// Gets or sets if spell checking is enabled.
        /// </summary>
        /// <value>
        /// Indicates if spell checking is enabled.</placeholder>
        /// </value>
        public bool? SpellCheck { get; set; }

        /// <summary>
        /// Gets or sets if the staging endpoint is used.
        /// </summary>
        /// <value>
        /// If the staging endpoint is used.
        /// </value>
        public bool? Staging { get; set; }

        /// <summary>
        /// Gets or sets the time zone offset.
        /// </summary>
        /// <value>
        /// The time zone offset.
        /// </value>
        public double? TimezoneOffset { get; set; }

        /// <summary>
        /// Gets or sets the verbose flag.
        /// </summary>
        /// <value>
        /// The verbose flag.
        /// </value>
        public bool? Verbose { get; set; }

        /// <summary>
        /// Gets or sets the Bing Spell Check subscription key.
        /// </summary>
        /// <value>
        /// The Bing Spell Check subscription key.
        /// </value>
        public string BingSpellCheckSubscriptionKey { get; set; }

        /// <summary>
        /// Gets or sets any extra query parameters for the URL.
        /// </summary>
        /// <value>
        /// Extra query parameters for the URL.
        /// </value>
        public string ExtraParameters { get; set; }

        /// <summary>
        /// Gets or sets the context id.
        /// </summary>
        /// <value>
        /// The context id.
        /// </value>
        [Obsolete("Action binding in LUIS should be replaced with code.")]
        public string ContextId { get; set; }

        /// <summary>
        /// Gets or sets force setting the parameter when using action binding.
        /// </summary>
        /// <value>
        /// Force setting the parameter when using action binding.
        /// </value>
        [Obsolete("Action binding in LUIS should be replaced with code.")]
        public string ForceSet { get; set; }

        /// <summary>
        /// Build the Uri for issuing the request for the specified Luis model.
        /// </summary>
        /// <param name="model"> The Luis model.</param>
        /// <returns> The request Uri.</returns>
        public Uri BuildUri(ILuisModel model)
        {
            if (model.ModelID == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "id");
            }

            if (model.SubscriptionKey == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "subscriptionKey");
            }

            var queryParameters = new List<string>
            {
                $"subscription-key={Uri.EscapeDataString(model.SubscriptionKey)}",
                $"q={Uri.EscapeDataString(Query)}",
            };
            UriBuilder builder;

            var id = Uri.EscapeDataString(model.ModelID);
            switch (model.ApiVersion)
            {
#pragma warning disable CS0612
                case LuisApiVersion.V1:
                    builder = new UriBuilder(model.UriBase);
                    queryParameters.Add($"id={id}");
                    break;
#pragma warning restore CS0612
                case LuisApiVersion.V2:
                    // v2.0 have the model as path parameter
                    builder = new UriBuilder(new Uri(model.UriBase, id));
                    break;
                default:
                    throw new ArgumentException($"{model.ApiVersion} is not a valid Luis api version.");
            }

            if (Log != null)
            {
                queryParameters.Add($"log={Uri.EscapeDataString(Convert.ToString(Log))}");
            }

            if (SpellCheck != null)
            {
                queryParameters.Add($"spellCheck={Uri.EscapeDataString(Convert.ToString(SpellCheck))}");
            }

            if (Staging != null)
            {
                queryParameters.Add($"staging={Uri.EscapeDataString(Convert.ToString(Staging))}");
            }

            if (TimezoneOffset != null)
            {
                queryParameters.Add($"timezoneOffset={Uri.EscapeDataString(Convert.ToString(TimezoneOffset))}");
            }

            if (Verbose != null)
            {
                queryParameters.Add($"verbose={Uri.EscapeDataString(Convert.ToString(Verbose))}");
            }

            if (!string.IsNullOrWhiteSpace(BingSpellCheckSubscriptionKey))
            {
                queryParameters.Add($"bing-spell-check-subscription-key={Uri.EscapeDataString(BingSpellCheckSubscriptionKey)}");
            }
#pragma warning disable CS0618
            if (ContextId != null)
            {
                queryParameters.Add($"contextId={Uri.EscapeDataString(ContextId)}");
            }

            if (ForceSet != null)
            {
                queryParameters.Add($"forceSet={Uri.EscapeDataString(ForceSet)}");
            }
#pragma warning restore CS0618
            if (ExtraParameters != null)
            {
                queryParameters.Add(ExtraParameters);
            }

            builder.Query = string.Join("&", queryParameters);
            return builder.Uri;
        }
    }

    /// <summary>
    /// A mockable interface for the LUIS service.
    /// </summary>
    public interface ILuisService
    {
        /// <summary>
        /// Modify the incoming LUIS request.
        /// </summary>
        /// <param name="request">Request so far.</param>
        /// <returns>Modified request.</returns>
        LuisRequest ModifyRequest(LuisRequest request);

        /// <summary>
        /// Build the query uri for the <see cref="LuisRequest"/>.
        /// </summary>
        /// <param name="luisRequest">The luis request text.</param>
        /// <returns>The query uri.</returns>
        Uri BuildUri(LuisRequest luisRequest);

        /// <summary>
        /// Query the LUIS service using this uri.
        /// </summary>
        /// <param name="uri">The query uri.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The LUIS result.</returns>
        Task<LuisResult> QueryAsync(Uri uri, CancellationToken token);
    }

    /// <summary>
    /// LUIS extension methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Query the LUIS service using this text.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="text">The query text.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The LUIS result.</returns>
        public static async Task<LuisResult> QueryAsync(this ILuisService service, string text, CancellationToken token)
        {
            var luisRequest = service.ModifyRequest(new LuisRequest(query: text));
            return await service.QueryAsync(luisRequest, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Query the LUIS service using this request.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="request">Query request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>LUIS result.</returns>
        public static async Task<LuisResult> QueryAsync(this ILuisService service, LuisRequest request, CancellationToken token)
        {
            service.ModifyRequest(request);
            var uri = service.BuildUri(request);
            return await service.QueryAsync(uri, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Builds luis uri with text query.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="text">The query text.</param>
        /// <returns>The LUIS request Uri.</returns>
        public static Uri BuildUri(this ILuisService service, string text)
        {
            return service.BuildUri(service.ModifyRequest(new LuisRequest(query: text)));
        }
    }

    /// <summary>
    /// Standard implementation of ILuisService against actual LUIS service.
    /// </summary>
    [Serializable]
    public sealed class LuisService : ILuisService
    {
        private static readonly HttpClient DefaultHttpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) };
        private readonly ILuisModel model;

        private HttpClient _httpClient = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisService"/> class using the model information.
        /// </summary>
        /// <param name="model">The LUIS model information.</param>
        /// <param name="customHttpClient">an optional alternate HttpClient.</param>
        public LuisService(ILuisModel model, HttpClient customHttpClient = null)
        {
            _httpClient = customHttpClient ?? DefaultHttpClient;
            SetField.NotNull(out this.model, nameof(model), model);
        }

        public static void Fix(LuisResult result)
        {
            // fix up Luis result for backward compatibility
            // v2 api is not returning list of intents if verbose query parameter
            // is not set. This will move IntentRecommendation in TopScoringIntent
            // to list of Intents.
            if (result.Intents == null || result.Intents.Count == 0)
            {
                if (result.TopScoringIntent != null)
                {
                    result.Intents = new List<IntentRecommendation> { result.TopScoringIntent };
                }
            }
        }

        public LuisRequest ModifyRequest(LuisRequest request) => model.ModifyRequest(request);

        Uri ILuisService.BuildUri(LuisRequest luisRequest) => luisRequest.BuildUri(this.model);

        public void ApplyThreshold(LuisResult result)
        {
            if (result.TopScoringIntent.Score > model.Threshold)
            {
                return;
            }

            result.TopScoringIntent.Intent = "None";
            result.TopScoringIntent.Score = 1.0d;
        }

        async Task<LuisResult> ILuisService.QueryAsync(Uri uri, CancellationToken token)
        {
            string json;
            using (var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead, token).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            try
            {
                var result = JsonConvert.DeserializeObject<LuisResult>(json);
                Fix(result);
                ApplyThreshold(result);
                return result;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Unable to deserialize the LUIS response.", ex);
            }
        }
    }
}
