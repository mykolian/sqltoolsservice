//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.SqlServer.Management.QueryStoreModel.Common;
using Microsoft.SqlServer.Management.QueryStoreModel.RegressedQueries;
using Microsoft.SqlTools.Hosting.Protocol.Contracts;

#nullable disable

namespace Microsoft.SqlTools.ServiceLayer.QueryStore.Contracts
{
    public class GetRegressedQueriesReportParams : QueryConfigurationParams<RegressedQueriesConfiguration>
    {
        TimeInterval TimeIntervalRecent;
        TimeInterval TimeIntervalHistory;
        long MinExecutionCount;

        public override RegressedQueriesConfiguration Convert()
        {
            RegressedQueriesConfiguration result = base.Convert();

            result.TimeIntervalRecent = TimeIntervalRecent;
            result.TimeIntervalHistory = TimeIntervalHistory;
            result.MinExecutionCount = MinExecutionCount;

            return result;
        }
    }

    /// <summary>
    /// Gets the Regressed Queries summary
    /// </summary>
    public class GetRegressedQueriesSummaryRequest
    {
        public static readonly RequestType<GetRegressedQueriesReportParams, QueryStoreQueryResult> Type
            = RequestType<GetRegressedQueriesReportParams, QueryStoreQueryResult>.Create("queryStore/getRegressedQueriesSummary");
    }

    /// <summary>
    /// Gets the Regressed Queries summary
    /// </summary>
    public class GetRegressedQueriesDetailedSummaryRequest
    {
        public static readonly RequestType<GetRegressedQueriesReportParams, QueryStoreQueryResult> Type
            = RequestType<GetRegressedQueriesReportParams, QueryStoreQueryResult>.Create("queryStore/getRegressedQueriesDetailedSummary");
    }
}
