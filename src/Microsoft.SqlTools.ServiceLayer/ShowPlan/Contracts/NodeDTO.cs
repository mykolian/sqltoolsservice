//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System.Collections.Generic;

namespace Microsoft.SqlTools.ServiceLayer.ShowPlan.Contracts
{
    public class NodeDTO
    {
        public List<NodeDTO> Children { get; set; } = new List<NodeDTO>();
        public double Cost { get; set; }
        public string Description { get; set; }
        public string DisplayCost { get; set; }
        public string DisplayName { get; set; }
        public List<ExecutionPlanEdges> Edges { get; set; } = new List<ExecutionPlanEdges>();
        public long? ElapsedTimeInMs { get; set; }
        public GraphDTO Graph { get; set; }
        public int GroupIndex { get; set; }
        public bool HasWarnings { get; set; }
        public int ID { get; set; }
        public bool IsParallel { get; set; }
        public string LogicalOpUnlocName { get; set; }
        public OperationDTO Operation { get; set; }
        public NodeDTO Parent { get; set; }
        public string PhysicalOpUnlocName { get; set; }
        public List<ExecutionPlanGraphPropertyBase> Properties { get; set; }
        public double RelativeCost { get; set; }
        public NodeDTO Root { get; set; }
        public double SubtreeCost { get; set; }

        public NodeDTO()
        { }
    }
}