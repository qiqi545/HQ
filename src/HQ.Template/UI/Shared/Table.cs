#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Lime;
using Lime.Web;
using Lime.Web.SemanticUi;
using TypeKitchen;

namespace HQ.Template.UI.Shared
{
    public class Table<T> : HtmlComponent<T> where T : IEnumerable
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<Type, ITypeReadAccessor> Accessors = new Dictionary<Type, ITypeReadAccessor>();

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<Type, AccessorMembers> Members = new Dictionary<Type, AccessorMembers>();

        static Table()
        {
            Accessors[typeof(T)] = ReadAccessor.Create(typeof(T), out var members);
            Members[typeof(T)] = members;
        }

        public override void Render(Ui ui, T rows)
        {
            var accessor = Accessors[typeof(T)];
            var columns = Members[typeof(T)];

            Header(ui, columns);

            ui.BeginElement("tbody");
            {
                foreach(var row in rows)
                {
                    ui.BeginElement("tr");
                    ui.BeginElement("td", new { @class = "collapsing" });
                    {
                        ui.BeginDiv("ui fitted slider checkbox")
                            .Input(InputType.Checkbox)
                            .Label("")
                            .EndDiv();
                    }
                    ui.EndElement("td");
                    foreach (var column in columns)
                    {
                        ui.BeginElement("td")
                            .Literal(accessor[row, column.Name]?.ToString());
                        ui.EndElement("td");
                    }
                    ui.EndElement("tr");
                }
            }
            ui.EndElement("tbody");

            Footer(ui);
        }

        private static void Footer(Ui ui)
        {
            ui.BeginTfoot("full-width");
            {
                ui.BeginElement("tr");
                {
                    ui.Element("th");
                    ui.BeginElement("th", new {colspan = 4});
                    {
                        ui.BeginDiv("ui right floated small primary labeled icon button");
                        ui.Icon(InterfaceIcons.User).Literal("Add User");
                        ui.EndDiv();

                        ui.BeginDiv("ui small button").Literal("Approve").EndDiv();
                        ui.BeginDiv("ui small button disabled").Literal("Approve All").EndDiv();
                    }
                    ui.EndElement("th");
                }
                ui.EndElement("tr");
            }
            ui.EndTfoot();
        }

        private static void Header(Ui ui, AccessorMembers members)
        {
            ui.BeginTable("ui compact celled definition table");
            {
                ui.BeginElement("thead", new {@class = "full-width"});
                {
                    ui.BeginElement("tr");
                    ui.Element("th");
                    foreach (var member in members)
                    {
                        ui.Element("th", member.Name, attr: null);
                    }

                    ui.EndElement("tr");
                }
                ui.EndElement("thead");
            }
            ui.EndTable();
        }
    }
}
