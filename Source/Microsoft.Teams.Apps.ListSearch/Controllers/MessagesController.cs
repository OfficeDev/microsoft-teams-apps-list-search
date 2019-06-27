// <copyright file="MessagesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using AdaptiveCards;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams;
    using Microsoft.Bot.Connector.Teams.Models;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;
    using Microsoft.Teams.Apps.ListSearch.Common.Helpers;
    using Microsoft.Teams.Apps.ListSearch.Common.Models;
    using Microsoft.Teams.Apps.ListSearch.Models;
    using Microsoft.Teams.Apps.ListSearch.Resources;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Activity = Microsoft.Bot.Connector.Activity;

    /// <summary>
    /// Messages Controller for the bot.
    /// </summary>
    public class MessagesController : ApiController
    {
        private const int HeightInPixels = 532;
        private const int WidthInPixels = 600;
        private const string AdaptiveCardVersion = "1.0";

        private readonly JwtHelper jwtHelper;
        private readonly ILogProvider logProvider;
        private readonly int jwtLifetimeInMinutes;
        private readonly string appBaseDomain;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesController"/> class.
        /// </summary>
        /// <param name="jwtHelper">instance of jwt helper</param>
        /// <param name="logProvider">instance of log provider</param>
        public MessagesController(JwtHelper jwtHelper, ILogProvider logProvider)
        {
            this.jwtHelper = jwtHelper;
            this.logProvider = logProvider;
            this.jwtLifetimeInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["JwtLifetimeInMinutes"]);
            this.appBaseDomain = ConfigurationManager.AppSettings["AppBaseDomain"];
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// <param name="activity">Activity from the UI.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        [Route("api/Messages")]
        public HttpResponseMessage Post([FromBody]Activity activity)
        {
            this.LogActivityTelemetry(activity);

            if (activity.Type == ActivityTypes.Invoke)
            {
                return this.HandleInvokeActivity(activity);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
        }

        // Handle incoming invoke activities
        private HttpResponseMessage HandleInvokeActivity(Activity activity)
        {
            if (activity.Name == "composeExtension/fetchTask")
            {
                string user = activity.From.Id;
                string tenant = activity.GetTenantId();
                string jwt = this.jwtHelper.GenerateJWT(activity.From.Id, activity.From.Properties["aadObjectId"].ToString(), activity.GetTenantId(), this.jwtLifetimeInMinutes);
                string sessionId = Guid.NewGuid().ToString();

                TaskInfo taskInfo = new TaskInfo()
                {
                    Url = $"https://{this.appBaseDomain}/search?token={jwt}&sessionId={sessionId}&theme={{theme}}",
                    Title = Strings.MessagingExtensionTitle,
                    Width = WidthInPixels,
                    Height = HeightInPixels,
                };
                TaskSubmitResponse taskEnvelope = new TaskSubmitResponse()
                {
                    Task = new TaskContinueResult(taskInfo),
                };

                // Log invocation of messaging extension
                this.logProvider.LogEvent("SearchSessionStarted", new Dictionary<string, string>
                {
                    { "SessionId", sessionId },
                });

                return this.Request.CreateResponse(HttpStatusCode.OK, taskEnvelope);
            }
            else if (activity.Name == "composeExtension/submitAction")
            {
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCaseExceptDictionaryKeysResolver(),
                    Formatting = Formatting.None,
                };
                var reply = ((JObject)activity.Value)["data"].ToString();
                SelectedSearchResult selectedSearchResult = JsonConvert.DeserializeObject<SelectedSearchResult>(reply, jsonSerializerSettings);

                List<AdaptiveFact> facts = new List<AdaptiveFact>();
                foreach (DeserializedAnswer child in selectedSearchResult.Answers)
                {
                    facts.Add(new AdaptiveFact()
                    {
                        Title = Convert.ToString(child.Question + ":"),
                        Value = Convert.ToString(child.Answer),
                    });
                }

                AdaptiveCard card = new AdaptiveCard(AdaptiveCardVersion)
                {
                    Body = new List<AdaptiveElement>()
                    {
                        new AdaptiveContainer()
                        {
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text = selectedSearchResult.Question,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Wrap = true,
                                    Size = AdaptiveTextSize.Large,
                                },
                            },
                        },
                        new AdaptiveContainer()
                        {
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveFactSet()
                                {
                                    Facts = facts ?? new List<AdaptiveFact>(),
                                },
                            },
                        },
                    },
                    Actions = new List<AdaptiveAction>()
                    {
                        new AdaptiveOpenUrlAction()
                        {
                            Url = new Uri(this.CreateListItemUrl(selectedSearchResult.SharePointListUrl, selectedSearchResult.ListItemId)),
                            Title = Strings.ResultCardButtonTitle,
                        },
                    },
                };

                var result = new Attachment()
                {
                    Content = card,
                    ContentType = AdaptiveCard.ContentType,
                };
                var preview = new ThumbnailCard
                {
                    Title = selectedSearchResult.Question,
                };

                ComposeExtensionResponse composeExtensionResponse = new ComposeExtensionResponse()
                {
                    ComposeExtension = new ComposeExtensionResult()
                    {
                        Attachments = new List<ComposeExtensionAttachment>() { result.ToComposeExtensionAttachment(preview.ToAttachment()) },
                        Type = ComposeExtensionResultType.Result,
                        AttachmentLayout = AttachmentLayoutTypes.List,
                    },
                };

                // Log that the search result was selected and turned into a card
                this.logProvider.LogEvent("SearchResultShared", new Dictionary<string, string>
                {
                    { "KnowledgeBaseId", selectedSearchResult.KBId },
                    { "ListItemId", selectedSearchResult.ListItemId },
                    { "SessionId", selectedSearchResult.SessionId },
                });

                return this.Request.CreateResponse(HttpStatusCode.OK, composeExtensionResponse);
            }

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        // Log telemetry about the incoming activity.
        private void LogActivityTelemetry(Activity activity)
        {
            var fromObjectId = activity.From?.Properties["aadObjectId"]?.ToString();
            var clientInfoEntity = activity.Entities?.Where(e => e.Type == "clientInfo")?.FirstOrDefault();
            var channelData = (JObject)activity.ChannelData;

            var properties = new Dictionary<string, string>
            {
                { "ActivityId", activity.Id },
                { "ActivityType", activity.Type },
                { "ActivityName", activity.Name },
                { "UserAadObjectId", fromObjectId },
                {
                    "ConversationType",
                    string.IsNullOrWhiteSpace(activity.Conversation?.ConversationType) ? "personal" : activity.Conversation.ConversationType
                },
                { "TeamId", channelData?["team"]?["id"]?.ToString() },
                { "SourceName", channelData?["source"]?["name"]?.ToString() },
                { "Locale", clientInfoEntity?.Properties["locale"]?.ToString() },
                { "Platform", clientInfoEntity?.Properties["platform"]?.ToString() },
            };
            this.logProvider.LogEvent("UserActivity", properties);
        }

        // Create URL to a SharePoint list item
        private string CreateListItemUrl(string listUrl, string itemId)
        {
            var itemUrl = listUrl;

            var finalSlashIndex = listUrl.LastIndexOf('/');
            if (finalSlashIndex > 0)
            {
                itemUrl = listUrl.Substring(0, finalSlashIndex) + "/DispForm.aspx?ID=" + itemId;
            }

            return listUrl;
        }
    }
}