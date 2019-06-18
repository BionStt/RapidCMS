﻿using System.Threading.Tasks;
using RapidCMS.Common.Data;
using RapidCMS.Common.Forms;
using RapidCMS.Common.Models.Commands;
using RapidCMS.Common.Models.UI;

namespace RapidCMS.Common.Services
{
    // TODO: make button handling more seperate
    // TODO: remove IRelationContainer and put in EditContext
    // TODO: why variantAlias for ProcessNodeEditorAction and not ProcessListAction?

    public interface ICollectionService
    {
        Task<ListUI> GetCollectionListViewAsync(string action, string collectionAlias, string? variantAlias, string? parentId);
        Task<ViewCommand> ProcessListActionAsync(string action, string collectionAlias, string? parentId, string actionId, object? customData);
        Task<ViewCommand> ProcessListActionAsync(string action, string collectionAlias, string? parentId, string id, EditContext editContext, IRelationContainer relationContainer, string actionId, object? customData);
        
        Task<NodeUI> GetNodeEditorAsync(string action, string collectionAlias, string variantAlias, string? parentId, string? id);
        Task<ViewCommand> ProcessNodeEditorActionAsync(string collectionAlias, string variantAlias, string? parentId, string? id, EditContext editContext, IRelationContainer relationContainer, string actionId, object? customData);
    }
}
