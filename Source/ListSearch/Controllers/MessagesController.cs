// <copyright file="MessagesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using AdaptiveCards;
    using Lib.Models;
    using ListSearch.Helpers;
    using ListSearch.Models;
    using ListSearch.Resources;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams;
    using Microsoft.Bot.Connector.Teams.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Activity = Microsoft.Bot.Connector.Activity;

    /// <summary>
    /// Messages Controller for the bot.
    /// </summary>
    public class MessagesController : ApiController
    {
        private readonly string messagingExtensionWidth = "medium";
        private readonly string adaptiveCardVersion = "1.0";

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// <param name="activity">Activity from the UI.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        [Route("api/Messages")]
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
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
                string jwt = JWTHelper.GenerateJWT(activity.From.Id, activity.From.Properties["aadObjectId"].ToString(), activity.GetTenantId());
                TaskInfo taskInfo = new TaskInfo()
                {
                    Url = $"{ConfigurationManager.AppSettings["WebAppUrl"]}/search/search?token={jwt}",
                    Title = Strings.MessagingExtensionTitle,
                    Width = this.messagingExtensionWidth
                };
                TaskSubmitResponse taskEnvelope = new TaskSubmitResponse()
                {
                    Task = new TaskContinueResult(taskInfo)
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, taskEnvelope);
            }
            else if (activity.Name == "composeExtension/submitAction")
            {
                SelectedSearchResult selectedSearchResult = JsonConvert.DeserializeObject<SelectedSearchResult>(((JObject)activity.Value)["data"].ToString());
                List<AdaptiveFact> facts = new List<AdaptiveFact>();
                foreach (DeserializedAnswer child in selectedSearchResult.Answers)
                {
                    facts.Add(new AdaptiveFact()
                    {
                        Title = Convert.ToString(child.Question),
                        Value = Convert.ToString(child.Answer),
                    });
                }

                AdaptiveCard card = new AdaptiveCard(this.adaptiveCardVersion)
                {
                    Body = new List<AdaptiveElement>()
                    {
                        new AdaptiveContainer()
                        {
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveColumnSet()
                                {
                                    Columns = new List<AdaptiveColumn>()
                                    {
                                        new AdaptiveColumn()
                                        {
                                            Width = AdaptiveColumnWidth.Auto,
                                            Items = new List<AdaptiveElement>()
                                            {
                                                new AdaptiveImage()
                                                {
                                                    Url = new Uri(ConfigurationManager.AppSettings["ListImageUrl"]),
                                                    Size = AdaptiveImageSize.Medium,
                                                }
                                            }
                                        },
                                        new AdaptiveColumn()
                                        {
                                            Width = AdaptiveColumnWidth.Stretch,
                                            Items = new List<AdaptiveElement>()
                                            {
                                                new AdaptiveTextBlock()
                                                {
                                                    Text = selectedSearchResult.KBName,
                                                    Weight = AdaptiveTextWeight.Bolder,
                                                    Wrap = true,
                                                }
                                            }
                                        }
                                    }
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
                            // TODO: Update with sharepoint item url.
                            Url = new Uri("https://www.bing.com"),
                            Title = "View More",
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
                        AttachmentLayout = AttachmentLayout.List
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