﻿using System;
using System.Collections.Generic;
using System.Linq;
using RapidCMS.Core.Abstractions.Resolvers;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Models.Config;

namespace RapidCMS.Core.Resolvers.Convention
{
    internal class ConventionBasedListConfigResolver : IConventionBasedResolver<ListConfig>
    {
        private readonly IFieldConfigResolver _fieldConfigResolver;

        public ConventionBasedListConfigResolver(IFieldConfigResolver fieldConfigResolver)
        {
            _fieldConfigResolver = fieldConfigResolver;
        }

        public ListConfig ResolveByConvention(Type subject, Features features)
        {
            var listButtons = new List<ButtonConfig>();

            if (features.HasFlag(Features.CanEdit) || features.HasFlag(Features.CanGoToEdit))
            {
                listButtons.Add(new DefaultButtonConfig
                {
                    ButtonType = DefaultButtonType.New
                });
            };
            var paneButtons = new List<ButtonConfig>();
            if (features.HasFlag(Features.CanGoToEdit))
            {
                paneButtons.Add(new DefaultButtonConfig
                {
                    ButtonType = DefaultButtonType.Edit
                });
            }
            if (features.HasFlag(Features.CanEdit))
            {
                paneButtons.Add(new DefaultButtonConfig
                {
                    ButtonType = DefaultButtonType.SaveExisting
                });
                paneButtons.Add(new DefaultButtonConfig
                {
                    ButtonType = DefaultButtonType.SaveNew
                });
                paneButtons.Add(new DefaultButtonConfig
                {
                    ButtonType = DefaultButtonType.Delete
                });
            }

            var result = new ListConfig(subject)
            {
                PageSize = 25,
                Buttons = listButtons,
                ListEditorType = ListType.Table,
                Panes = new List<PaneConfig>
                {
                    new PaneConfig(subject)
                    {
                        Buttons =  paneButtons,
                        FieldIndex = 1,
                        Fields = _fieldConfigResolver.GetFields(subject, features).ToList(),
                        VariantType = subject
                    }
                },
                ReorderingAllowed = false,
                SearchBarVisible = true
            };

            return result;
        }
    }
}
