using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using Services.Dto;

namespace FileMonitor
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
            // Split the file path components then add them to a Queue.
            var pathElements = new Queue<string>(dto.Path.Split(Path.DirectorySeparatorChar).ToList());
            AddNodes(pathElements, FileTree.Items);
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
            var pathElements = dto.Path.Split(Path.DirectorySeparatorChar);
            if (PathExists(pathElements, FileTree.Items))
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
        private void AddNodes(Queue<string> pathElements, ItemCollection childItems)
        {
            if (pathElements.Count == 0) return;
            var first = new TreeViewItem();
            TreeViewItem? match;
            first.Header = pathElements.Dequeue();

            if (TryGetMatch(childItems, first, out match))
            {
                AddNodes(pathElements, match.Items);
            }
            else
            {
                childItems.Add(first);
                AddNodes(pathElements, first.Items);
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

        private bool PathExists(string[] pathElements, ItemCollection childItems)
        {
            bool result = false;
            foreach(var elem in pathElements)
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
    }
}
