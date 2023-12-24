using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using Services.Dto;

namespace FileMonitor.View
{
    /// <summary>
    /// A class for creating a Windows File Explorer tree view
    /// </summary>
    public class FileExplorerTreeView
    {
        private ObservableCollection<IPathNode> _rootNodes;
        private ReadOnlyObservableCollection<IPathNode> _readOnlyRootNodes;
        private List<IPathDto> _paths;

        /// <summary>
        /// Holds a readonly collection of all root nodes for this instance of the tree view. 
        /// </summary>
        public ReadOnlyObservableCollection<IPathNode> RootNodes => _readOnlyRootNodes;

        /// <summary>
        /// Holds an <see cref="IEnumerable{T}"/> of all data transfer objects added to this instance of the 
        /// tree view. Use this property to access the full paths or the database IDs. 
        /// </summary>
        public IEnumerable<IPathDto> FullPaths => _paths;

        public enum NodeCategory
        {
            Root = 0,
            Directory = 1,
            File = 2
        }

        /// <summary>
        /// The <see cref="FileExplorerTreeView"/> class constructor. 
        /// </summary>
        public FileExplorerTreeView(IEnumerable<IPathDto> paths)
        {
            _rootNodes = new ObservableCollection<IPathNode>();
            _readOnlyRootNodes = new ReadOnlyObservableCollection<IPathNode>(_rootNodes);
            _paths = new List<IPathDto>();
            AddPaths(paths);
        }

        /// <summary>
        /// Add a single path to the tree view.
        /// </summary>
        public void AddPath(IPathDto dto)
        {
            _paths.Add(dto);
            var pathNodes = ToQueue(dto);
            AddNodes(ref pathNodes, ref _rootNodes, dto.Id, default);
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
            // Get Path as DTO
            //var dto = GetPath(node);
            var node = GetNode(dto, _rootNodes);
            if(node != null)
            {
            _paths.Remove(dto);
            RemoveNodes(node);
        }
        }

        /// <summary>
        /// Remove multiple paths from the tree view.
        /// </summary>
        public void RemovePaths(IEnumerable<IPathDto> dtos)
        {
            foreach (var dto in dtos) RemovePath(dto);
        }

        /// <summary>
        /// Get a path DTO from the given node.
        /// </summary>
        public IPathDto? GetPath(IPathNode node)
        {
            return _paths
                .Where(path => path.Id == node.PathId)
                .FirstOrDefault();
        }

        public IEnumerable<IPathNode> GetNodes(Func<IPathNode, bool> predicate)
        {
            var values = new List<IPathNode>();
            return GetNodes(predicate, _rootNodes, ref values);
        }

        // Create a recursive solution to get a node from the given predicate
        private IEnumerable<IPathNode> GetNodes(
            Func<IPathNode, bool> predicate,
            IEnumerable<IPathNode> rootNodes,
            ref List<IPathNode> values)
        {
            foreach (var node in rootNodes)
            {
                var children = node.Children;
                if(children.Count == 0) 
                    return values;
                values.AddRange(children.Where(predicate));
                return GetNodes(predicate, children, ref values);
            }
            return values;
        }

        private static Queue<IPathNode> ToQueue(IPathDto dto)
        {
            string fileName;
            var attr = File.GetAttributes(dto.Path);

            if(attr.HasFlag(FileAttributes.Directory))
            {
                fileName = "";
            }
            else fileName = Path.GetFileName(dto.Path);

            var pathElements = dto.Path.Split(Path.DirectorySeparatorChar);
            var root = Path.GetPathRoot(dto.Path);
            var queue = new Queue<IPathNode>();

            var node = new PathNode(root, NodeCategory.Root);
            queue.Enqueue(node);

            foreach (var elem in pathElements)
            {
                var formatted = $"{elem}{Path.DirectorySeparatorChar}";

                if (formatted == root)
                    continue;
                
                if (formatted != fileName)
                {
                    node = new PathNode(
                        formatted,
                        NodeCategory.Directory);
                    queue.Enqueue(node);
                    continue;
                }

                node = new PathNode(
                    formatted,
                    NodeCategory.File);
                queue.Enqueue(node);
            }
            return queue;
        }



        // Add each file path node recursively to the TreeView.
        private void AddNodes(
            ref Queue<IPathNode> pathNodes, 
            ref ObservableCollection<IPathNode> childItems, 
            int pathId, 
            IPathNode? parent)
        {
            bool returnToCaller = false;
            if (pathNodes.Count == 1) returnToCaller = true;

            IPathNode? first;
            IPathNode? match;
            first = pathNodes.Dequeue();
            first.Parent = parent;

            if (TryGetMatch(ref childItems, first, out match))
            {
                if (returnToCaller) return;
                var children = match.Children;
                parent = match;
                AddNodes(ref pathNodes, ref children, pathId, parent);
            }
            else
            {
                if (returnToCaller)
                {
                    // Only display the checkbox on the final path node. This allows the user to delete the path based
                    // on the last node. Additionally, store the pathId on the final node, so when the user wants to 
                    // delete a path, the ID is sent back to the services layer. 
                    first.DisplayCheckBox = true;
                    first.PathId = pathId;
                    childItems.Add(first);
                    return;
                }
                childItems.Add(first);
                var children = first.Children;
                parent = first;
                AddNodes(ref pathNodes, ref children, pathId, parent);
            }
        }

        // If the TreeViewItem is contained in childItems, return true and return the "item" object as an out
        // parameter.
        private bool TryGetMatch(ref ObservableCollection<IPathNode> childItems, IPathNode item, out IPathNode? match)
        {
            bool result = false;
            match = default;

            foreach (var childItem in childItems)
            {
                if (!item.Text.Equals(childItem.Text))
                    continue;

                match = childItem;
                result = true;  
                break;
            }
            return result;
        }

        private void RemoveNodes(IPathNode node)
        {
            // Remove the node. If parent is null, then the node is at the root of list, therefore remove the node from
            // _rootNodes. Contine to Walk through the tree in reverse and remove any additional nodes as long as they
            // contain no logical children. When parent == null, you've reached the root of the list
            var parent = node.Parent;
            if (parent != null)
            {
                parent.Children.Remove(node);
                node = parent;
                parent = node.Parent;
            }
            else
            {
                _rootNodes.Remove(node);
                return;
            }

            while (parent != null)
            {
                if(node.Children.Count == 0)
                    parent.Children.Remove(node);
                node = parent;
                parent = node.Parent;
                if (parent == null)
                {
                    if (node.Children.Count == 0) 
                        _rootNodes.Remove(node);
                }
            }
        }

        private class PathNode : IPathNode
        {
            public string? Text { get; set; }
            public NodeCategory Category { get; set; }
            public bool DisplayCheckBox { get; set; }
            public bool IsChecked { get; set; }
            public ObservableCollection<IPathNode>? Children { get; set; }
            public IPathNode Parent { get; set; }
            public int PathId { get; set; }

            public override string? ToString() => Text;

            public PathNode(string text, NodeCategory category, PathNode parent = null)
            {
                Text = text;
                Category = category;
                Parent = parent;
                PathId = -1;
                Children = new ObservableCollection<IPathNode>();
            }
        }
    }
}
