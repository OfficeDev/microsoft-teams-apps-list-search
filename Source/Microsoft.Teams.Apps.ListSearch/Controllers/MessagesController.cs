// <copyright file="MessagesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using AdaptiveCards;
    using ListSearch.Common.Helpers;
    using ListSearch.Common.Models;
    using ListSearch.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams;
    using Microsoft.Bot.Connector.Teams.Models;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesController"/> class.
        /// </summary>
        /// <param name="jwtHelper">instance of jwt helper</param>
        public MessagesController(JwtHelper jwtHelper)
        {
            this.jwtHelper = jwtHelper;
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
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (activity.Type == ActivityTypes.Invoke)
            {
                return this.HandleInvokeMessages(activity);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
        }

        private HttpResponseMessage HandleInvokeMessages(Activity activity)
        {
            if (activity.Name == "composeExtension/fetchTask")
            {
                string user = activity.From.Id;
                string tenant = activity.GetTenantId();
                string jwt = this.jwtHelper.GenerateJWT(activity.From.Id, activity.From.Properties["aadObjectId"].ToString(), activity.GetTenantId(), Convert.ToInt32(ConfigurationManager.AppSettings["JwtLifetimeInMinutes"]));

                TaskInfo taskInfo = new TaskInfo()
                {
                    Url = $"https://{ConfigurationManager.AppSettings["AppBaseDomain"]}/search/search?token={jwt}&theme={{theme}}",
                    Title = Strings.MessagingExtensionTitle,
                    Width = WidthInPixels,
                    Height = HeightInPixels,
                };
                TaskSubmitResponse taskEnvelope = new TaskSubmitResponse()
                {
                    Task = new TaskContinueResult(taskInfo)
                };
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

                string sharePointUrl = selectedSearchResult.SharePointURL;
                sharePointUrl = sharePointUrl.Replace("AllItems.aspx", $"DispForm.aspx?ID={selectedSearchResult.Id}");

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
                                Size = AdaptiveTextSize.Large
                            },
                        }
                    },
                    new AdaptiveContainer()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveFactSet()
                            {
                                Facts = facts ?? new List<AdaptiveFact>(),
                            }
                        }
                    }
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveOpenUrlAction()
                    {
                        Url = new Uri(sharePointUrl),
                        Title = Strings.ResultCardButtonTitle,
                    }
                }
                };

                Attachment attachment = new Attachment()
                {
                    Content = card,
                    ContentType = AdaptiveCard.ContentType
                };

                ComposeExtensionResponse composeExtensionResponse = new ComposeExtensionResponse()
                {
                    ComposeExtension = new ComposeExtensionResult()
                    {
                        Attachments = new List<ComposeExtensionAttachment>() { attachment.ToComposeExtensionAttachment() },
                        Type = ComposeExtensionResultType.TaskResult,
                        AttachmentLayout = AttachmentLayoutTypes.List
                    }
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, composeExtensionResponse);
            }

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing the the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}