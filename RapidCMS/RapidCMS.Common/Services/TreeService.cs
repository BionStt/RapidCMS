﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RapidCMS.Common.Authorization;
using RapidCMS.Common.Data;
using RapidCMS.Common.Enums;
using RapidCMS.Common.Extensions;
using RapidCMS.Common.Helpers;
using RapidCMS.Common.Models;
using RapidCMS.Common.Models.UI;

namespace RapidCMS.Common.Services
{
    internal class TreeService : ITreeService
    {
        private readonly Root _root;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public TreeService(Root root, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService)
        {
            _root = root;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public async Task<TreeUI?> GetTreeAsync(string alias, string? parentId)
        {
            var collection = _root.GetCollection(alias);
            var testEntity = await collection.Repository.InternalNewAsync(parentId, collection.EntityVariant.Type);

            var viewAuthorizationChallenge = await _authorizationService.AuthorizeAsync(
                _httpContextAccessor.HttpContext.User,
                testEntity,
                Operations.Read);

            var editAuthorizationChallenge = await _authorizationService.AuthorizeAsync(
                _httpContextAccessor.HttpContext.User,
                testEntity,
                Operations.Update);

            var tree = new TreeUI
            {
                Alias = collection.Alias,
                Name = collection.Name,
                EntitiesVisible = collection.TreeView?.EntityVisibility == EntityVisibilty.Visible,
                RootVisible = collection.TreeView?.RootVisibility == CollectionRootVisibility.Visible
            };

            if (collection.ListEditor != null && editAuthorizationChallenge.Succeeded)
            {
                tree.Icon = "list-rich";
                tree.Path = UriHelper.Collection(Constants.Edit, collection.Alias, parentId);
            }
            else if (collection.ListView != null && viewAuthorizationChallenge.Succeeded)
            {
                tree.Icon = "list";
                tree.Path = UriHelper.Collection(Constants.List, collection.Alias, parentId);
            }

            return tree;
        }

        public async Task<List<TreeNodeUI>> GetNodesAsync(string alias, string? parentId, Action? onNodesUpdated = null)
        {
            var collection = _root.GetCollection(alias);

            if (collection.TreeView?.EntityVisibility == EntityVisibilty.Visible)
            {
                // TODO: pagination
                var query = Query.TakeElements(25);
                var entities = await collection.Repository.InternalGetAllAsync(parentId, query);

                if (onNodesUpdated != null)
                {
                    // TODO: update this to return Idisposable to the registerer (Nodes.razor)
                    collection.Repository.ChangeToken.RegisterChangeCallback((x) => onNodesUpdated.Invoke(), null);
                }

                return await entities.ToListAsync(async entity =>
                {
                    var entityVariant = collection.GetEntityVariant(entity);

                    var node = new TreeNodeUI
                    {
                        Id = entity.Id,
                        Name = collection.TreeView.Name!.StringGetter.Invoke(entity),
                        Collections = collection.Collections.ToList(subCollection => subCollection.Alias)
                    };

                    var editAuthorizationChallenge = await _authorizationService.AuthorizeAsync(
                        _httpContextAccessor.HttpContext.User,
                        entity,
                        Operations.Update);

                    if (editAuthorizationChallenge.Succeeded)
                    {
                        node.Path = UriHelper.Node(Constants.Edit, collection.Alias, entityVariant, parentId, entity.Id);
                    }
                    else
                    { 
                        var viewAuthorizationChallenge = await _authorizationService.AuthorizeAsync(
                            _httpContextAccessor.HttpContext.User,
                            entity,
                            Operations.Read);

                        if (viewAuthorizationChallenge.Succeeded)
                        {
                            node.Path = UriHelper.Node(Constants.View, collection.Alias, entityVariant, parentId, entity.Id);
                        }
                    }

                    return node;
                });
            }
            else
            {
                return new List<TreeNodeUI>();
            }
        }

        public TreeRootUI GetRoot()
        {
            var collections = _root.Collections.Select(x => x.Alias).ToList();

            return new TreeRootUI
            {
                Collections = collections,
                Name = _root.SiteName
            };
        }
    }
}
