using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// A class for creating a Windows File Explorer tree view.
    /// </summary>
    public class FileExplorerTreeView
    {
        /// <summary>
        /// The public <see cref="TreeView"/> property to bind to the UI.
        /// </summary>
        public TreeView FileTree { get; }

        /// <summary>
        /// The <see cref="FileExplorerTreeView"/> class constructor. 
        /// </summary>
        public FileExplorerTreeView()
        {
            FileTree = new TreeView();
        }

        /// <summary>
        /// Add a single path to the <see cref="TreeView"/>.
        /// </summary>
        public void AddPath(string path)
        {
            // Split the file path components then add them to a Queue.
            var pathElements = new Queue<string>(path.Split(Path.DirectorySeparatorChar).ToList());
            AddNodes(pathElements, FileTree.Items);
        }

        /// <summary>
        /// Add multiple paths to the <see cref="TreeView"/>.
        /// </summary>
        public void AddPaths(IEnumerable<string> paths)
        {
            foreach (var path in paths) AddPath(path);
        }

        // Add each file path element recursively to the TreeView.
        private void AddNodes(Queue<string> pathElements, ItemCollection childItems)
        {
            if (pathElements.Count == 0) return;
            var first = new TreeViewItem();
            TreeViewItem? match;
            first.Header = pathElements.Dequeue();

            if (HasItem(childItems, first, out match))
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
        private bool HasItem(ItemCollection childItems, TreeViewItem item, out TreeViewItem? match)
        {
            foreach (var childItem in childItems)
            {
                var cast = childItem as TreeViewItem;
                if (cast == null)
                {
                    match = null;
                    return false;
                }
                if (item.Header.Equals(cast.Header))
                {
                    match = cast;
                    return true;
                }
            }
            match = null;
            return false;
        }
    }
}
