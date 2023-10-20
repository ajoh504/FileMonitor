using System.Collections.Generic;
using System.Collections;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using Services.Dto;

namespace FileMonitor.View
{
    /// <summary>
    /// A class for creating a Windows File Explorer tree view.
    /// </summary>
    public class FileExplorerTreeView
    {
        private TreeView FileTree { get; }

        /// <summary>
        /// A public collection exposing an <see cref="ItemCollection"/> of <see cref="TreeViewItem"/>s.
        /// </summary>
        public IEnumerable FileTreeItems => FileTree.Items;

        /// <summary>
        /// The <see cref="FileExplorerTreeView"/> class constructor. 
        /// </summary>
        public FileExplorerTreeView()
        {
            FileTree = new TreeView();
        }

        /// <summary>
        /// Add a single path to the tree view.
        /// </summary>
        public void AddPath(IPathDto dto)
        {
            var pathNodes = ToQueue(dto);
            AddNodes(pathNodes, FileTree.Items);
        }

        /// <summary>
        /// Add multiple paths to the tree view.
        /// </summary>
        public void AddPaths(IEnumerable<IPathDto> dtos)
        {
            foreach (var dto in dtos) AddPath(dto);
        }

        /// <summary>
        /// Remove a single path from the tree view.
        /// </summary>
        public void RemovePath(IPathDto dto)
        {
            var pathNodes = dto.Path.Split(Path.DirectorySeparatorChar);
            if (PathExists(pathNodes, FileTree.Items))
                Debug.WriteLine("TEST OUTPUT: RESULT = TRUE");
        }

        /// <summary>
        /// Remove multiple paths from the tree view.
        /// </summary>
        public void RemovePaths(IEnumerable<IPathDto> dtos)
        {
            foreach (var dto in dtos) RemovePath(dto);
        }

        // Add each file path element recursively to the TreeView.
        private void AddNodes(Queue<PathNode> pathNodes, ItemCollection childItems)
        {
            if (pathNodes.Count == 0) return;
            var first = new TreeViewItem();
            TreeViewItem? match;
            first.Header = pathNodes.Dequeue();

            if (TryGetMatch(childItems, first, out match))
            {
                AddNodes(pathNodes, match.Items);
            }
            else
            {
                childItems.Add(first);
                AddNodes(pathNodes, first.Items);
            }
        }

        // If the TreeViewItem is contained in childItems, return true and return the "item" object as an out
        // parameter.
        private bool TryGetMatch(ItemCollection childItems, TreeViewItem item, out TreeViewItem? match)
        {
            bool result = false;
            match = default;

            foreach (var childItem in childItems)
            {
                if(childItem is TreeViewItem treeViewItem)
                {
                    if (!item.Header.Equals(treeViewItem.Header))
                        continue;

                    match = treeViewItem;
                    result = true; 
                }
                break;
            }
            return result;
        }

        // Returns true if the path exists in this tree view instance, false otherwise.
        private bool PathExists(string[] pathNodes, ItemCollection childItems)
        {
            bool result = false;
            foreach(var elem in pathNodes)
            {
                var item = new TreeViewItem();
                item.Header = elem;
                TreeViewItem? match;

                if (TryGetMatch(childItems, item, out match))
                {
                    childItems = match.Items;
                    result = true;
                }
                else
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        // Returns a Queue of PathNodes to be added to this instance of the tree view. 
        private static Queue<PathNode> ToQueue(IPathDto dto)
        {
            var pathElements = dto.Path.Split(Path.DirectorySeparatorChar);
            var root = Path.GetPathRoot(dto.Path);
            var fileName = Path.GetFileName(dto.Path);
            var queue = new Queue<PathNode>();
            PathNode node;

            foreach (var element in pathElements)
            {
                if(element + "\\" == root)
                {
                    node = new(element, PathNode.NodeCategory.Root);
                    queue.Enqueue(node);
                    continue;
                }
                else if (element + "\\" != fileName)
                {
                    node = new(element, PathNode.NodeCategory.Directory);
                    queue.Enqueue(node);
                    continue;
                }
                node = new(element, PathNode.NodeCategory.File);
                queue.Enqueue(node);
            }
            return queue;
        }

        class PathNode
        {
            public string? NodeItem { get; set; }
            public NodeCategory Category { get; set; }

            public override string? ToString() => NodeItem;

            public enum NodeCategory
            {
                Root = 0,
                Directory = 1,
                File = 2
            }

            public PathNode(string nodeItem, NodeCategory category)
            {
                NodeItem = nodeItem;
                Category = category;
            }
        }
    }
}
