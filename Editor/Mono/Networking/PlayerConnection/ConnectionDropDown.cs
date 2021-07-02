// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Networking.PlayerConnection
{
    internal static class ConnectionUIHelper
    {
        private static string portPattern = @":\d{4,}";
        private static string ipPattern = @"@\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}";
        private static string localHostPattern = @" (Localhost prohibited)";
        public static readonly string kTetheredDevices = L10n.Tr("Tethered Devices");

        public static string GetToolbarContent(string connectionName)
        {
            var projectNameFromConnectionIdentifier = GetProjectNameFromConnectionIdentifier(ProfilerDriver.connectedProfiler);
            return $"{GetPlayerNameFromIDString(GetIdString(connectionName))}{(string.IsNullOrEmpty(projectNameFromConnectionIdentifier) ? "" : $" - {projectNameFromConnectionIdentifier}")}";
        }

        public static string GetIdString(string id)
        {
            //strip port info
            var portMatch = Regex.Match(id, portPattern);
            if (portMatch.Success)
            {
                id = id.Replace(portMatch.Value, "");
            }

            //strip ip info
            var ipMatch = Regex.Match(id, ipPattern);
            if (ipMatch.Success)
            {
                id = id.Replace(ipMatch.Value, "");
            }

            var removedWarning = false;
            if (id.Contains(localHostPattern))
            {
                id = id.Replace(localHostPattern, "");
                removedWarning = true;
            }

            id = id.Contains('(') ? id.Substring(id.IndexOf('(') + 1) : id;// trim the brackets
            id = id.EndsWith(")") ? id.Substring(0, id.Length - 1) : id; // trimend cant be used as the player id might end with )

            if (removedWarning)
                id += localHostPattern;
            return id;
        }

        public static string GetRuntimePlatformFromIDString(string id)
        {
            var s = id.Contains(',') ? id.Substring(0, id.IndexOf(',')) : "";

            return RuntimePlatform.TryParse(s, out RuntimePlatform result) ? result.ToString() : "";
        }

        public static string GetPlayerNameFromIDString(string id)
        {
            return id.Contains(',') ? id.Substring(id.IndexOf(',') + 1) : id;
        }

        public static string GetPlayerType(string connectionName)
        {
            return connectionName.Contains('(')
                ? connectionName.Substring(0, connectionName.IndexOf('('))
                : connectionName;
        }

        public static string GetProjectNameFromConnectionIdentifier(int connectionId)
        {
            return ProfilerDriver.GetProjectName(connectionId);
        }

        public static string GetIP(int connectionId)
        {
            return ProfilerDriver.GetConnectionIP(connectionId);
        }

        public static string GetPort(int connectionId)
        {
            var port = ProfilerDriver.GetConnectionPort(connectionId);
            return port == 0 ? "-" : port.ToString();
        }

        public static GUIContent GetIcon(string name)
        {
            // *begin-nonstandard-formatting*
            var content = name switch
            {
                "Editor" => EditorGUIUtility.IconContent("d_SceneAsset Icon"),
                "WindowsEditor" => EditorGUIUtility.IconContent("d_SceneAsset Icon"),
                "WindowsPlayer" => EditorGUIUtility.IconContent("BuildSettings.Metro.Small"),
                "Android" => EditorGUIUtility.IconContent("BuildSettings.Android.Small"),
                "OSXPlayer" => EditorGUIUtility.IconContent("BuildSettings.Standalone.Small"),
                "IPhonePlayer" => EditorGUIUtility.IconContent("BuildSettings.iPhone.Small"),
                "WebGLPlayer" => EditorGUIUtility.IconContent("BuildSettings.WebGL.Small"),
                "tvOS" => EditorGUIUtility.IconContent("BuildSettings.tvOS.Small"),
                "Lumin" => EditorGUIUtility.IconContent("BuildSettings.Lumin.small"),
                "LinuxPlayer" => EditorGUIUtility.IconContent("BuildSettings.EmbeddedLinux.Small"),
                "WSAPlayerX86" => EditorGUIUtility.IconContent("BuildSettings.Metro.Small"),
                "WSAPlayerX64" => EditorGUIUtility.IconContent("BuildSettings.Metro.Small"),
                "WSAPlayerARM" => EditorGUIUtility.IconContent("BuildSettings.Metro.Small"),
                "Switch" => EditorGUIUtility.IconContent("BuildSettings.Switch.Small"),
                "Stadia" => EditorGUIUtility.IconContent("BuildSettings.Stadia.small"),
                "GameCoreScarlett" => EditorGUIUtility.IconContent("BuildSettings.GameCoreScarlett.Small"),
                "GameCoreXboxOne" => EditorGUIUtility.IconContent("BuildSettings.GameCoreXboxOne.Small"),
                "XboxOne" => EditorGUIUtility.IconContent("BuildSettings.GameCoreXboxOne.Small"),
                "EmbeddedLinuxArm64" => EditorGUIUtility.IconContent("BuildSettings.EmbeddedLinux.Small"),
                "EmbeddedLinuxArm32" => EditorGUIUtility.IconContent("BuildSettings.EmbeddedLinux.Small"),
                "EmbeddedLinuxX64" => EditorGUIUtility.IconContent("BuildSettings.EmbeddedLinux.Small"),
                "EmbeddedLinuxX86" => EditorGUIUtility.IconContent("BuildSettings.EmbeddedLinux.Small"),
                "PS4" => EditorGUIUtility.IconContent("BuildSettings.PS4.Small"),
                "PS5" => EditorGUIUtility.IconContent("BuildSettings.PS5.Small"),
                "Tethered Devices" => EditorGUIUtility.IconContent("BuildSettings.Standalone.Small"),
                "<unknown>" => EditorGUIUtility.IconContent("BuildSettings.Broadcom"),
                _ => EditorGUIUtility.IconContent("BuildSettings.Broadcom")
            };
            // *end-nonstandard-formatting*
            return content;
        }
    }

    internal class ConnectionDropDownItem : TreeViewItem
    {
        public Func<bool> m_Connected;
        public Action m_OnSelected;
        public string m_SubGroup;
        public Func<bool> m_Disabled;
        public string ProjectName;
        public float ProjectNameSize;
        public ConnectionMajorGroup m_TopLevelGroup;
        public int m_ConnectionId;
        internal string DisplayName;
        public float DisplayNameSize;
        internal string IP;
        internal float IPSize;
        internal string Port;
        internal float PortSize;
        internal bool OldConnectionFormat;
        // *begin-nonstandard-formatting*
        public static GUIStyle sMenuItem => s_MenuItem ??= "TV Line";
        // *end-nonstandard-formatting*
        private static GUIStyle s_MenuItem;
        public bool IsDevice;
        public GUIContent IconContent;

        static class Content
        {
            public static readonly string DirectConnection = L10n.Tr("Direct Connection");
        }

        internal enum ConnectionMajorGroup
        {
            Logging,
            Local,
            Remote,
            ConnectionsWithoutID,
            Direct,
            Unknown
        }

        internal static string[] ConnectionMajorGroupLabels =
        {
            "Logging",
            "Local",
            "Remote",
            "Connections Without ID",
            "Direct Connection",
            "Unknown"
        };

        public ConnectionDropDownItem(string content, int connectionId, string subGroup, ConnectionMajorGroup topLevelGroup, Func<bool> connected, Action onSelected,
                                      Func<bool> disable, bool isDevice = false, string iconString = null)
        {
            var idString = ConnectionUIHelper.GetIdString(content);
            DisplayName = subGroup == ConnectionUIHelper.kTetheredDevices ? content : ConnectionUIHelper.GetPlayerNameFromIDString(idString);
            DisplayNameSize = sMenuItem.CalcSize(GUIContent.Temp(DisplayName)).x;

            ProjectName = subGroup == ConnectionUIHelper.kTetheredDevices ? "" : ConnectionUIHelper.GetProjectNameFromConnectionIdentifier(connectionId);
            ProjectNameSize = sMenuItem.CalcSize(GUIContent.Temp(ProjectName)).x;

            IP = ConnectionUIHelper.GetIP(connectionId);
            IPSize = sMenuItem.CalcSize(GUIContent.Temp(IP)).x;

            Port = ConnectionUIHelper.GetPort(connectionId);
            PortSize = sMenuItem.CalcSize(GUIContent.Temp(Port)).x;

            m_ConnectionId = connectionId;
            m_Disabled = disable;
            m_Connected = connected;
            m_OnSelected = onSelected;
            m_SubGroup = string.IsNullOrEmpty(subGroup) ? ConnectionUIHelper.GetRuntimePlatformFromIDString(idString) : subGroup;
            m_TopLevelGroup = topLevelGroup == ConnectionMajorGroup.Unknown ? (m_SubGroup == Content.DirectConnection ? ConnectionMajorGroup.Direct : (ProfilerDriver.IsIdentifierOnLocalhost(connectionId) ? ConnectionMajorGroup.Local : ConnectionMajorGroup.Remote)) : topLevelGroup;

            if (m_TopLevelGroup == ConnectionMajorGroup.Direct)
            {
                DisplayName = content;
            }

            if (string.IsNullOrEmpty(m_SubGroup))
            {
                if (idString == "Editor" || idString == "Main Editor Process")
                {
                    m_TopLevelGroup = ConnectionMajorGroup.Local;
                    m_SubGroup = "Editor";
                }
                else
                {
                    DisplayName = content;
                    m_TopLevelGroup = ConnectionMajorGroup.ConnectionsWithoutID;
                    OldConnectionFormat = true;
                }
            }

            displayName = DisplayName + ProjectName;
            id = GetHashCode();
            IsDevice = isDevice;
            if (isDevice)
            {
                IconContent = ConnectionUIHelper.GetIcon(iconString);
            }
        }

        public override int GetHashCode()
        {
            return (DisplayName + ProjectName + IP + Port).GetHashCode();
        }

        public bool DisplayProjectName()
        {
            return !IsDevice && !OldConnectionFormat &&
                ConnectionMajorGroup.Direct != m_TopLevelGroup && ConnectionMajorGroup.Logging != m_TopLevelGroup;
        }

        public bool DisplayPort()
        {
            return !IsDevice && !OldConnectionFormat &&
                ConnectionMajorGroup.Direct != m_TopLevelGroup && ConnectionMajorGroup.Logging != m_TopLevelGroup;
        }

        public bool DisplayIP()
        {
            return !IsDevice && !OldConnectionFormat &&
                ConnectionMajorGroup.Direct != m_TopLevelGroup && ConnectionMajorGroup.Logging != m_TopLevelGroup;
        }
    }

    enum ConnectionDropDownColumns
    {
        DisplayName,
        ProjectName,
        IP,
        Port
    }

    internal class ConnectionDropDownMultiColumnHeader : MultiColumnHeader
    {
        public ConnectionDropDownMultiColumnHeader(MultiColumnHeaderState state) : base(state)
        {
        }

        protected override void AddColumnHeaderContextMenuItems(GenericMenu menu)
        {
            base.AddColumnHeaderContextMenuItems(menu);
            menu.menuItems.RemoveAt(0);
        }
    }

    internal class ConnectionTreeView : TreeView
    {
        public List<ConnectionDropDownItem> dropDownItems = new List<ConnectionDropDownItem>(0);
        private readonly Color SeparatorColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public string search = "";
        private const int ToggleRectWidth = 16;
        private const float SeparatorLineWidth = 1f;
        private const float SeparatorVerticalPadding = 2f;

        public ConnectionTreeView(TreeViewState state, ConnectionDropDownMultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            showAlternatingRowBackgrounds = true;
            baseIndent = 0;
            depthIndentWidth = foldoutWidth;
            rowHeight = 20f;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            foreach (var connectionType in dropDownItems.OrderBy(x => x.m_TopLevelGroup).GroupBy(x => x.m_TopLevelGroup))
            {
                // connection major group
                var i = new TreeViewItem { displayName = ConnectionDropDownItem.ConnectionMajorGroupLabels[(int)connectionType.Key]};
                i.id = (i.displayName + root.displayName).GetHashCode();
                root.AddChild(i);
                foreach (var playerType in connectionType.Where(x => x.m_SubGroup != null).GroupBy(x => x.m_SubGroup).OrderBy(x => x.Key))
                {
                    // direct and preeasy id connections dont have any subgrouping
                    if (i.displayName == ConnectionDropDownItem.ConnectionMajorGroupLabels[(int)ConnectionDropDownItem.ConnectionMajorGroup.Direct] ||
                        i.displayName == ConnectionDropDownItem.ConnectionMajorGroupLabels[(int)ConnectionDropDownItem.ConnectionMajorGroup.ConnectionsWithoutID])
                    {
                        foreach (var player in playerType.OrderBy(x => x.DisplayName).Where(x => x.DisplayName.ToLower().Contains(search.ToLower())))
                            i.AddChild(player);
                        continue;
                    }

                    // if we match a top level group add all of its children and return
                    if (string.Equals(i.displayName.ToLower(), search.ToLower(), StringComparison.Ordinal))
                    {
                        AddChildren(playerType, i);
                        SetupDepthsFromParentsAndChildren(root);
                        return root;
                    }
                    // if we match a player type add all of its children and continue
                    if (string.Equals(playerType.Key.ToLower(), search.ToLower(), StringComparison.Ordinal))
                    {
                        AddChildren(playerType, i);
                        continue;
                    }

                    AddChildren(playerType, i, search);
                }

                if (!i.hasChildren)
                    root.children.Remove(i);
            }
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        TreeViewItem AddHeaderItem(string name, TreeViewItem item)
        {
            if (name == item.displayName) return item;

            var newItem = new TreeViewItem {displayName = name, id = (name + item.parent.displayName).GetHashCode()};
            item.AddChild(newItem);
            return newItem;
        }

        void AddChildren(IGrouping<string, ConnectionDropDownItem> group, TreeViewItem treeViewItem, string filter = null)
        {
            if (filter == null)
            {
                var header = AddHeaderItem(group.Key, treeViewItem);
                foreach (var player in group.OrderBy(x => x.DisplayName))
                    header.AddChild(player);
            }
            else
            {
                var header = AddHeaderItem(group.Key, treeViewItem);
                bool addedChild = false;
                foreach (var player in group.Where(x => x.DisplayName.ToLower().Contains(filter.ToLower()))
                         .OrderBy(x => x.DisplayName))
                {
                    addedChild = true;
                    header.AddChild(player);
                }

                if (!addedChild && treeViewItem.hasChildren)
                    treeViewItem.children.Remove(treeViewItem.children.Last());
            }
        }

        protected override void SingleClickedItem(int id)
        {
            var t = dropDownItems.Find(x => x.id == id);
            if (t is ConnectionDropDownItem && (!t.m_Disabled?.Invoke() ?? true))
                t.m_OnSelected?.Invoke();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;

            if (item is ConnectionDropDownItem cddi)
            {
                GUI.enabled = (!cddi.m_Disabled?.Invoke() ?? true);
                if (GUI.enabled && args.selected && Event.current.keyCode == KeyCode.Return)
                {
                    cddi.m_OnSelected?.Invoke();
                    Repaint();
                }
            }

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (ConnectionDropDownColumns)args.GetColumn(i), ref args);
            }
            GUI.enabled = true;
        }

        void CellGUI(Rect cellRect, TreeViewItem item, ConnectionDropDownColumns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);
            ConnectionDropDownItem cddi = null;
            if (item is ConnectionDropDownItem downItem)
                cddi = downItem;

            switch (column)
            {
                case ConnectionDropDownColumns.DisplayName:
                    if (cddi != null)
                    {//actual connections
                        var rect = cellRect;
                        rect.x += GetContentIndent(item) - foldoutWidth;

                        EditorGUI.BeginChangeCheck();
                        rect.width = ToggleRectWidth;
                        var isConnected = cddi.m_Connected?.Invoke() ?? false;
                        GUI.Label(rect, (isConnected ? EditorGUIUtility.IconContent("Valid") : GUIContent.none));
                        rect.x += ToggleRectWidth;
                        if (cddi.IsDevice)
                        {
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rowHeight, rowHeight), cddi.IconContent);
                            rect.x += rowHeight;
                        }
                        var textRect = cellRect;
                        textRect.x = rect.x;
                        textRect.width = cellRect.width - (textRect.x - cellRect.x);
                        GUI.Label(textRect,  cddi.DisplayName);

                        if (EditorGUI.EndChangeCheck())
                        {
                            cddi.m_OnSelected.Invoke();
                        }
                    }
                    else
                    {
                        var r = cellRect;
                        if (item.depth <= 0)
                        {// major group headers
                            if (args.row != 0)
                                EditorGUI.DrawRect(new Rect(r.x, r.y, args.rowRect.width, 1f), SeparatorColor);
                            r.x += GetContentIndent(item);
                            r.width = args.rowRect.width - GetContentIndent(item);
                            GUI.Label(r, item.displayName, EditorStyles.boldLabel);
                        }
                        else
                        {// sub group headers
                            r.x += GetContentIndent(item);
                            EditorGUI.LabelField(new Rect(r.x, r.y, rowHeight, rowHeight), ConnectionUIHelper.GetIcon(item.displayName));
                            GUI.Label(new Rect(r.x + rowHeight, r.y, r.width - rowHeight, r.height), item.displayName, EditorStyles.miniBoldLabel);
                        }
                    }
                    break;

                case ConnectionDropDownColumns.ProjectName:
                    if (cddi?.DisplayProjectName() ?? false)
                    {
                        DrawVerticalSeparatorLine(cellRect);
                        GUI.Label(cellRect, cddi.ProjectName);
                    }
                    break;
                case ConnectionDropDownColumns.IP:
                    if (cddi?.DisplayIP() ?? false)
                    {
                        DrawVerticalSeparatorLine(cellRect);
                        GUI.Label(cellRect, cddi.IP);
                    }
                    break;
                case ConnectionDropDownColumns.Port:
                    if (cddi?.DisplayPort() ?? false)
                    {
                        DrawVerticalSeparatorLine(cellRect);
                        GUI.Label(cellRect, cddi.Port);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(column), column, null);
            }
        }

        private void DrawVerticalSeparatorLine(Rect cellRect)
        {
            EditorGUI.DrawRect(
                new Rect(cellRect.x - 2, cellRect.y + SeparatorVerticalPadding, SeparatorLineWidth,
                    cellRect.height - 2 * SeparatorVerticalPadding), SeparatorColor);
        }

        public float GetHeight()
        {
            if (dropDownItems.Count > 0)
                return totalHeight;

            return 0;
        }
    }

    internal class ConnectionTreeViewWindow : PopupWindowContent
    {
        private ConnectionTreeView m_connectionTreeView;
        private SearchField m_SearchField;
        private float searchFieldPadding = 12f;
        private float searchFieldVerticalSpacing = 12f;
        private ConnectionDropDownMultiColumnHeader multiColumnHeader;
        private IConnectionStateInternal state;
        private static MultiColumnHeaderState multiColumnHeaderState;
        private static TreeViewState treeViewState;
        private List<ConnectionDropDownItem> connectionItems;
        private static bool firstOpen = true;

        public ConnectionTreeViewWindow(IConnectionStateInternal internalState, Rect rect)
        {
            state = internalState;
            state.AddItemsToMenu(this, rect);
            if (multiColumnHeaderState == null)
            {
                treeViewState = new TreeViewState();
                multiColumnHeaderState = CreateDefaultMultiColumnHeaderState(100);
                multiColumnHeader = new ConnectionDropDownMultiColumnHeader(multiColumnHeaderState);
                m_connectionTreeView = new ConnectionTreeView(treeViewState, multiColumnHeader) { dropDownItems = connectionItems };
                SetMinColumnWidths();
                return;
            }

            multiColumnHeader = new ConnectionDropDownMultiColumnHeader(multiColumnHeaderState);
            m_connectionTreeView = new ConnectionTreeView(treeViewState, multiColumnHeader) { dropDownItems = connectionItems };
        }

        static class Content
        {
            private static GUIStyle style = MultiColumnHeader.DefaultStyles.columnHeader;
            public static GUIContent PlayerName = new GUIContent("Player Name");
            public static float PlayerNameMinWidth = style.CalcSize(PlayerName).x;
            public static GUIContent ProjectName = new GUIContent("Product Name");
            public static float ProjectNameMinWidth = style.CalcSize(ProjectName).x;
            public static GUIContent IP = new GUIContent("IP");
            public static float IPMinWidth = style.CalcSize(IP).x;
            public static GUIContent Port = new GUIContent("Port");
            public static float PortMinWidth = style.CalcSize(Port).x;
        }

        public override void OnClose()
        {
            state = null;
        }

        internal void Reload()
        {
            if (m_connectionTreeView.dropDownItems.Count > 0)
            {
                m_connectionTreeView.Reload();
            }
        }

        public override void OnOpen()
        {
            Reload();

            if (firstOpen)
                m_connectionTreeView.ExpandAll();

            firstOpen = false;
            m_SearchField = new SearchField();
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUI.BeginChangeCheck();
            m_connectionTreeView.search = m_SearchField.OnGUI(new Rect(rect.x + searchFieldPadding, rect.y + searchFieldPadding, rect.width - (2 * searchFieldPadding), EditorGUI.kSingleLineHeight), m_connectionTreeView.search);
            if (EditorGUI.EndChangeCheck())
            {
                Reload();
            }
            rect.y += EditorGUI.kSingleLineHeight + searchFieldVerticalSpacing;

            m_connectionTreeView?.OnGUI(new Rect(rect.x, rect.y, rect.width, (float)m_connectionTreeView?.totalHeight));
            rect.y += (float)m_connectionTreeView?.totalHeight;
        }

        private void SetMinColumnWidths()
        {
            foreach (var stateVisibleColumn in m_connectionTreeView.multiColumnHeader.state.visibleColumns)
            {
                m_connectionTreeView.multiColumnHeader.state.columns[stateVisibleColumn].width = GetLargestWidth(stateVisibleColumn);
            }
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = Content.PlayerName,
                    headerTextAlignment = TextAlignment.Left,
                    autoResize = false,
                    allowToggleVisibility = false,
                    minWidth = Content.PlayerNameMinWidth,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = Content.ProjectName,
                    headerTextAlignment = TextAlignment.Left,
                    autoResize = false,
                    allowToggleVisibility = true,
                    minWidth = Content.ProjectNameMinWidth,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = Content.IP,
                    headerTextAlignment = TextAlignment.Left,
                    autoResize = false,
                    allowToggleVisibility = true,
                    minWidth = Content.IPMinWidth,
                    canSort = false,
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = Content.Port,
                    headerTextAlignment = TextAlignment.Left,
                    autoResize = false,
                    allowToggleVisibility = true,
                    minWidth = Content.PortMinWidth,
                    canSort = false
                }
            };

            var state = new MultiColumnHeaderState(columns);
            return state;
        }

        public void AddDisabledItem(ConnectionDropDownItem connectionDropDownItem)
        {
            connectionDropDownItem.m_Disabled = () => true;
            AddItem(connectionDropDownItem);
        }

        public void AddItem(ConnectionDropDownItem connectionDropDownItem)
        {
            // *begin-nonstandard-formatting*
            connectionItems ??= new List<ConnectionDropDownItem>();
            // *end-nonstandard-formatting*
            // this is a hack to show switch connected over ethernet in tethered devices
            if (connectionDropDownItem.IP == "127.0.0.1" && ProfilerDriver.GetConnectionIdentifier(connectionDropDownItem.m_ConnectionId).StartsWith("Switch"))
            {
                connectionDropDownItem.m_TopLevelGroup = ConnectionDropDownItem.ConnectionMajorGroup.Local;
                connectionDropDownItem.m_SubGroup = "Tethered Devices";
                connectionDropDownItem.IsDevice = true;
                connectionDropDownItem.IconContent = ConnectionUIHelper.GetIcon("Switch");
                var fullName = ProfilerDriver.GetConnectionIdentifier(connectionDropDownItem.m_ConnectionId);
                var start = fullName.IndexOf('-') + 1;
                var end = fullName.IndexOf('(');
                connectionDropDownItem.DisplayName = $"{fullName.Substring(start, end - start)} - {ProfilerDriver.GetProjectName(connectionDropDownItem.m_ConnectionId)}";
            }

            var dupes = connectionItems.FirstOrDefault(x => x.DisplayName == connectionDropDownItem.DisplayName && x.IP == connectionDropDownItem.IP && x.Port == connectionDropDownItem.Port);
            if (dupes != null)
                connectionItems.Remove(dupes);

            connectionItems.Add(connectionDropDownItem);
        }

        public bool HasItem(string name)
        {
            return connectionItems != null && connectionItems.Any(x => x.DisplayName == name);
        }

        public override Vector2 GetWindowSize()
        {
            Vector2 v = Vector2.zero;
            v.y +=  EditorGUI.kSingleLineHeight + searchFieldVerticalSpacing; // search field
            v.x = Mathf.Max(200, GetTotalVisibleWidth());
            v.y += GetTotalTreeViewHeight();
            return v;
        }

        private float GetTotalTreeViewHeight()
        {
            return m_connectionTreeView.GetHeight();
        }

        float GetLargestWidth(int idx)
        {
            // *begin-nonstandard-formatting*
            return idx switch
            {
                0 => GetRequiredDisplayNameColumnWidth(),
                1 => GetRequiredProjectNameColumnWidth(),
                2 => GetRequiredIPColumnWidth(),
                3 => GetRequiredPortColumnWidth(),
                _ => 0
            };
            // *end-nonstandard-formatting*
        }

        private float GetRequiredProjectNameColumnWidth()
        {
            return Mathf.Max(Content.ProjectNameMinWidth,
                connectionItems.Max(x => x.ProjectNameSize));
        }

        private float GetRequiredDisplayNameColumnWidth()
        {
            return Mathf.Max(Content.PlayerNameMinWidth,
                connectionItems.Max(x => x.DisplayNameSize) + 20);
        }

        private float GetRequiredIPColumnWidth()
        {
            return Mathf.Max(Content.IPMinWidth, connectionItems.Max(x => x.IPSize));
        }

        private float GetRequiredPortColumnWidth()
        {
            return Mathf.Max(Content.PortMinWidth, connectionItems.Max(x => x.PortSize));
        }

        float GetTotalVisibleWidth()
        {
            return m_connectionTreeView.multiColumnHeader.state.widthOfAllVisibleColumns;
        }

        //needed for tests
        public int GetItemCount()
        {
            return m_connectionTreeView?.dropDownItems.Count ?? 0;
        }

        public void Clear()
        {
            m_connectionTreeView?.dropDownItems.Clear();
        }
    }
}