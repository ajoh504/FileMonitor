using System.Collections.Generic;
using System.IO;
using Services.Dto;
using System.Collections.ObjectModel;

namespace FileMonitor.View
{
    /// <summary>
    /// A class for creating a Windows File Explorer tree view.
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
            AddNodes(ref pathNodes, ref _rootNodes, dto.Id);
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
        public void RemovePath(IPathNode node)
        {
            // todo: update class to remove paths by their nodes
            //_paths.Remove(node);
            //var pathNodes = dto.Path.Split(Path.DirectorySeparatorChar);
            //RemoveNodes(pathNodes, dto);
        }

        /// <summary>
        /// Remove multiple paths from the tree view.
        /// </summary>
        public void RemovePaths(IEnumerable<IPathDto> dtos)
        {
            // todo: update class to remove paths by their nodes
            //foreach (var dto in dtos) RemovePath(dto);
        }

        private static Queue<IPathNode> ToQueue(IPathDto dto)
        {
            var pathElements = dto.Path.Split(Path.DirectorySeparatorChar);
            var root = Path.GetPathRoot(dto.Path);
            var fileName = Path.GetFileName(dto.Path);
            var queue = new Queue<IPathNode>();

            var node = new PathNode(root, NodeCategory.Root);
            var parent = node;
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
                        NodeCategory.Directory,
                        parent);
                    parent = node;
                    queue.Enqueue(node);
                    continue;
                }

                node = new PathNode(
                    formatted,
                    NodeCategory.File,
                    parent);
                queue.Enqueue(node);
            }
            return queue;
        }



        // Add each file path node recursively to the TreeView.
        private void AddNodes(ref Queue<IPathNode> pathNodes, ref ObservableCollection<IPathNode> childItems, int pathId)
        {
            bool returnToCaller = false;
            if (pathNodes.Count == 1) returnToCaller = true;

            IPathNode? first;
            IPathNode? match;
            first = pathNodes.Dequeue();

            if (TryGetMatch(ref childItems, first, out match))
            {
                if (returnToCaller) return;
                var children = match.Children;
                AddNodes(ref pathNodes, ref children, pathId);
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
                AddNodes(ref pathNodes, ref children, pathId);
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

        private void RemoveNodes(string[] pathNodes, IPathDto dto)
        {
            //var children = _rootNodes;
            //PathNode? match;
            //PathNode? parent;

            //    if (TryGetMatch(ref childItems, item, out _))
            //    {
            //        result = true;
            //    }
            //    else
            //    {
            //        result = false;
            //        break;
            //    }
            //}
            //return result;
            //foreach (var pathNode in pathNodes)
            //{
            //    if (TryGetMatch(children, new PathNode(pathNode), out match))
            //    {
            //        children = match.Children;
            //        // Check if the node is the last in the list
            //        if(match.PathId == dto.Id)
            //        {
            //            match.
            //        }
            //    }
            //    else break;
            //}
        }

        private class PathNode : IPathNode
        {
            public string? Text { get; set; }
            public NodeCategory Category { get; set; }
            public bool DisplayCheckBox { get; set; }
            public ObservableCollection<IPathNode>? Children { get; set; }
            public PathNode Parent { get; set; }
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
