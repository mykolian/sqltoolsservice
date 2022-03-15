//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System;
using System.Threading.Tasks;
using Microsoft.SqlTools.Hosting.Protocol;
using Microsoft.SqlTools.ServiceLayer.Hosting;
using Microsoft.SqlTools.ServiceLayer.ExecutionPlan.Contracts;
using Microsoft.SqlTools.ServiceLayer.ExecutionPlan.ExecPlanGraph;
using Microsoft.SqlTools.ServiceLayer.ExecutionPlan.ExecPlanGraph.Comparison;

namespace Microsoft.SqlTools.ServiceLayer.ExecutionPlan
{
    /// <summary>
    /// Main class for Migration Service functionality
    /// </summary>
    public sealed class ExecutionPlanService : IDisposable
    {
        private static readonly Lazy<ExecutionPlanService> instance = new Lazy<ExecutionPlanService>(() => new ExecutionPlanService());

        private bool disposed;

        /// <summary>
        /// Construct a new MigrationService instance with default parameters
        /// </summary>
        public ExecutionPlanService()
        {
        }

        /// <summary>
        /// Gets the singleton instance object
        /// </summary>
        public static ExecutionPlanService Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Service host object for sending/receiving requests/events.
        /// Internal for testing purposes.
        /// </summary>
        internal IProtocolEndpoint ServiceHost { get; set; }

        /// <summary>
        /// Initializes the ShowPlan Service instance
        /// </summary>
        public void InitializeService(ServiceHost serviceHost)
        {
            ServiceHost = serviceHost;
            ServiceHost.SetRequestHandler(GetExecutionPlanRequest.Type, HandleGetExecutionPlan);
            ServiceHost.SetRequestHandler(CreateSkeletonRequest.Type, HandleCreateSkeletonRequest);
            ServiceHost.SetRequestHandler(GraphComparisonRequest.Type, HandleGraphComparisonRequest);
            ServiceHost.SetRequestHandler(ColorMatchingSectionsRequest.Type, HandleColorMatchingRequest);
            ServiceHost.SetRequestHandler(FindNextNonIgnoreNodeRequest.Type, HandleFindNextNonIgnoreNodeRequest);
        }

        private async Task HandleGetExecutionPlan(GetExecutionPlanParams requestParams, RequestContext<GetExecutionPlanResult> requestContext)
        {
            try
            {
                var plans = ShowPlanGraphUtils.CreateShowPlanGraph(requestParams.GraphInfo.GraphFileContent, "");
                await requestContext.SendResult(new GetExecutionPlanResult
                {
                    Graphs = plans
                });
            }
            catch (Exception e)
            {
                await requestContext.SendError(e.ToString());
            }
        }

        /// <summary>
        /// Handles requests to create skeletons.
        /// </summary>
        internal async Task HandleCreateSkeletonRequest(
            CreateSkeletonParams parameter,
            RequestContext<CreateSkeletonResult> requestContext)
        {
            try
            {
                var graph = ExecPlanGraph.ExecutionPlanGraph.ParseShowPlanXML(parameter.QueryPlanXmlText, ShowPlanType.Unknown);
                var root = graph?[0]?.Root;

                var manager = new SkeletonManager();
                var skeletonNode = manager.CreateSkeleton(root);

                var result = new CreateSkeletonResult()
                {
                    SkeletonNode = skeletonNode.ConvertToDTO()
                };

                await requestContext.SendResult(result);
            }
            catch (Exception e)
            {
                await requestContext.SendError(e.ToString());
            }
        }

        /// <summary>
        /// Handles requests to compare graphs
        /// </summary>
        internal async Task HandleGraphComparisonRequest(
            GetGraphComparisonParams parameter,
            RequestContext<GetGraphComparisonResult> requestContext)
        {
            try
            {
                var firstGraphSet = ExecPlanGraph.ExecutionPlanGraph.ParseShowPlanXML(parameter.FirstQueryPlanXmlText, ShowPlanType.Unknown);
                var firstRootNode = firstGraphSet?[0]?.Root;

                var secondGraphSet = ExecPlanGraph.ExecutionPlanGraph.ParseShowPlanXML(parameter.SecondQueryPlanXmlText, ShowPlanType.Unknown);
                var secondRootNode = secondGraphSet?[0]?.Root;

                var manager = new SkeletonManager();
                var firstSkeletonNode = manager.CreateSkeleton(firstRootNode);
                var secondSkeletonNode = manager.CreateSkeleton(secondRootNode);
                var isEquivalent = manager.AreSkeletonsEquivalent(firstSkeletonNode, secondSkeletonNode, parameter.IgnoreDatabaseName);

                var result = new GetGraphComparisonResult()
                {
                    IsEquivalent = isEquivalent
                };

                await requestContext.SendResult(result);
            }
            catch (Exception e)
            {
                await requestContext.SendError(e.ToString());
            }
        }

        /// <summary>
        /// Handles requests for color matching similar nodes.
        /// </summary>
        internal async Task HandleColorMatchingRequest(
            ColorMatchingSectionsParams parameter,
            RequestContext<ColorMatchingSectionsResult> requestContext)
        {
            try
            {
                var firstGraphSet = ExecPlanGraph.ExecutionPlanGraph.ParseShowPlanXML(parameter.FirstQueryPlanXmlText, ShowPlanType.Unknown);
                var firstRootNode = firstGraphSet?[0]?.Root;

                var secondGraphSet = ExecPlanGraph.ExecutionPlanGraph.ParseShowPlanXML(parameter.SecondQueryPlanXmlText, ShowPlanType.Unknown);
                var secondRootNode = secondGraphSet?[0]?.Root;

                var manager = new SkeletonManager();
                var firstSkeletonNode = manager.CreateSkeleton(firstRootNode);
                var secondSkeletonNode = manager.CreateSkeleton(secondRootNode);
                manager.ColorMatchingSections(firstSkeletonNode, secondSkeletonNode, parameter.IgnoreDatabaseName);

                var firstSkeletonNodeDTO = firstSkeletonNode.ConvertToDTO();
                var secondSkeletonNodeDTO = secondSkeletonNode.ConvertToDTO();
                ShowPlanGraphUtils.CopyMatchingNodesIntoSkeletonDTO(firstSkeletonNodeDTO, secondSkeletonNodeDTO);
                ShowPlanGraphUtils.CopyMatchingNodesIntoSkeletonDTO(secondSkeletonNodeDTO, firstSkeletonNodeDTO);

                var result = new ColorMatchingSectionsResult()
                {
                    FirstSkeletonNode = firstSkeletonNodeDTO,
                    SecondSkeletonNode = secondSkeletonNodeDTO
                };

                await requestContext.SendResult(result);
            }
            catch (Exception e)
            {
                await requestContext.SendError(e.ToString());
            }
        }

        /// <summary>
        /// Handles request to locate the next node that should not be
        /// ignored during show plan comparisons.
        /// </summary>
        internal async Task HandleFindNextNonIgnoreNodeRequest(
            FindNextNonIgnoreNodeParams parameter,
            RequestContext<FindNextNonIgnoreNodeResult> requestContext)
        {
            try
            {
                var graph = ExecPlanGraph.ExecutionPlanGraph.ParseShowPlanXML(parameter.QueryPlanXmlText, ShowPlanType.Unknown);
                var root = graph?[0]?.Root;
                var startingNode = root.FindNodeById(parameter.StartingNodeID);

                if (startingNode == null)
                    await requestContext.SendError("Could not locate the starting node using the provided node ID.");

                var manager = new SkeletonManager();
                var nextNonIgnoreNode = manager.FindNextNonIgnoreNode(startingNode);

                var result = new FindNextNonIgnoreNodeResult()
                {
                    NextNonIgnoreNode = nextNonIgnoreNode.ConvertToDTO()
                };

                await requestContext.SendResult(result);
            }
            catch (Exception e)
            {
                await requestContext.SendError(e.ToString());
            }
        }

        /// <summary>
        /// Disposes the ShowPlan Service
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
            }
        }
    }
}