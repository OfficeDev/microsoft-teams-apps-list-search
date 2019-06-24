// <copyright file="RefreshController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;
    using Microsoft.Teams.Apps.ListSearch.Common.Helpers;
    using Microsoft.Teams.Apps.ListSearch.Common.Models;
    using Microsoft.Teams.Apps.ListSearch.Filters;

    /// <summary>
    /// Controller to refresh the KB.
    /// </summary>
    public class RefreshController : ApiController
    {
        private readonly KBInfoHelper kbInfoHelper;
        private readonly KnowledgeBaseRefreshHelper refreshHelper;
        private readonly ILogProvider logProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshController"/> class.
        /// </summary>
        /// <param name="kbInfoHelper">KB helper to use</param>
        /// <param name="refreshHelper">KB refresh helper to use</param>
        /// <param name="logProvider">Log provider to be used</param>
        public RefreshController(KBInfoHelper kbInfoHelper, KnowledgeBaseRefreshHelper refreshHelper, ILogProvider logProvider)
        {
            this.kbInfoHelper = kbInfoHelper;
            this.refreshHelper = refreshHelper;
            this.logProvider = logProvider;
        }

        /// <summary>
        /// Refreshes all KBs due for a refresh
        /// </summary>
        /// <returns><see cref="Task"/> to refresh KBs.</returns>
        [HttpPost]
        [RefreshAuthFilter]
        public async Task RefreshAllKBs()
        {
            this.logProvider.LogInfo("Refreshing all knowledge bases");

            List<KBInfo> kbList = await this.kbInfoHelper.GetAllKBs(
                fields: new string[]
                {
                    nameof(KBInfo.LastRefreshDateTime),
                    nameof(KBInfo.RefreshFrequencyInHours),
                    nameof(KBInfo.SharePointListId),
                    nameof(KBInfo.QuestionField),
                    nameof(KBInfo.AnswerFields),
                    nameof(KBInfo.SharePointSiteId),
                });
            this.logProvider.LogInfo($"Found {kbList.Count} configured KBs");

            foreach (var kb in kbList)
            {
                DateTime lastRefreshed = kb.LastRefreshDateTime;
                int frequencyInHours = kb.RefreshFrequencyInHours;
                if (lastRefreshed == DateTime.MinValue || frequencyInHours == 0)
                {
                    continue;
                }

                if (lastRefreshed.AddHours(frequencyInHours) < DateTime.UtcNow)
                {
                    try
                    {
                        await this.refreshHelper.RefreshKnowledgeBaseAsync(kb);

                        var properties = new Dictionary<string, string>
                        {
                            { "KnowledgeBaseId", kb.KBId },
                        };
                        this.logProvider.LogEvent("KnowledgeBaseRefreshSuccess", properties);
                    }
                    catch (Exception ex)
                    {
                        var properties = new Dictionary<string, string>
                        {
                            { "KnowledgeBaseId", kb.KBId },
                            { "LastRefreshDateTime", lastRefreshed.ToString("u") },
                            { "ErrorMessage", ex.Message },
                        };
                        this.logProvider.LogEvent("KnowledgeBaseRefreshFailure", properties);

                        this.logProvider.LogWarning($"Failed to refresh KB {kb.KBId}: {ex.Message}", exception: ex);
                        continue;
                    }
                }
                else
                {
                    this.logProvider.LogInfo($"Skipping refresh for {kb.KBId}, refreshed less than {kb.RefreshFrequencyInHours} hours ago");
                }
            }
        }
    }
}
