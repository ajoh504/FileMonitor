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
        private ObservableCollection<PathNode> _rootNodes;
        private ReadOnlyObservableCollection<PathNode> _readOnlyRootNodes;
        private List<IPathDto> _paths;

        /// <summary>
        /// Holds a readonly collection of all root nodes for this instance of the tree view. 
        /// </summary>
        public ReadOnlyObservableCollection<PathNode> RootNodes => _readOnlyRootNodes;

        /// <summary>
        /// Holds an <see cref="IEnumerable{T}"/> of all data transfer objects added to this instance of the 
        /// tree view. Use this property to access the full paths or the database IDs. 
        /// </summary>
        public IEnumerable<IPathDto> FullPaths => _paths;

        /// <summary>
        /// The <see cref="FileExplorerTreeView"/> class constructor. 
        /// </summary>
        public FileExplorerTreeView(IEnumerable<IPathDto> paths)
        {
            _rootNodes = new ObservableCollection<PathNode>();
            _readOnlyRootNodes = new ReadOnlyObservableCollection<PathNode>(_rootNodes);
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
            AddNodes(ref pathNodes, ref _rootNodes);
            AddNodes(pathNodes, _rootNodes, dto.Id);
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
            _paths.Remove(dto);
            var pathNodes = dto.Path.Split(Path.DirectorySeparatorChar);
            RemoveNodes(pathNodes, dto);
        }

        /// <summary>
        /// Remove multiple paths from the tree view.
        /// </summary>
        public void RemovePaths(IEnumerable<IPathDto> dtos)
        {
            foreach (var dto in dtos) RemovePath(dto);
        }

        // Returns a Queue of PathNodes to be added to this instance of the tree view. 
        private static Queue<PathNode> ToQueue(IPathDto dto)
        {
            var pathElements = dto.Path.Split(Path.DirectorySeparatorChar);
            var root = Path.GetPathRoot(dto.Path);
            var fileName = Path.GetFileName(dto.Path);
            var queue = new Queue<PathNode>();
            var node = new PathNode(root, PathNode.NodeCategory.Root);
            var parent = node;
            queue.Enqueue(node);

            foreach (var pathElement in pathElements)
            {
                if ($"{pathElement}{Path.DirectorySeparatorChar}" != fileName)
                {
                    node = new PathNode(
                        pathElement,
                        PathNode.NodeCategory.Directory,
                        parent);
                    parent = node;
                    queue.Enqueue(node);
                    continue;
                }
                node = new PathNode(
                    pathElement,
                    PathNode.NodeCategory.File,
                    parent);
                queue.Enqueue(node);
            }
            return queue;
        }

        // Add each file path node recursively to the TreeView.
        private void AddNodes(ref Queue<PathNode> pathNodes, ref ObservableCollection<PathNode> childItems)
        private void AddNodes(Queue<PathNode> pathNodes, ObservableCollection<PathNode> childItems, int pathId)
        {
            bool returnToCaller = false;
            if (pathNodes.Count == 1) returnToCaller = true;

            PathNode? first;
            PathNode? match;
            first = pathNodes.Dequeue();

            if (TryGetMatch(ref childItems, first, out match))
            {
                if (returnToCaller) return;
                var children = match.Children;
                AddNodes(ref pathNodes, ref children);
                AddNodes(pathNodes, match.Children, pathId);
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
                AddNodes(ref pathNodes, ref children);
                AddNodes(pathNodes, first.Children, pathId);
            }
        }

        // If the TreeViewItem is contained in childItems, return true and return the "item" object as an out
        // parameter.
        private bool TryGetMatch(ref ObservableCollection<PathNode> childItems, PathNode item, out PathNode? match)
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

                if (TryGetMatch(ref childItems, item, out _))
                {
                    result = true;
                }
                else
                {
                    result = false;
                    break;
                }
            }
            return result;
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

        public class PathNode
        {
            public string? Text { get; set; }
            public NodeCategory Category { get; set; }
            public bool DisplayCheckBox { get; set; }
            public ObservableCollection<PathNode>? Children { get; set; }
            public PathNode Parent { get; set; }
            public int PathId { get; set; }

            public override string? ToString() => Text;

            public enum NodeCategory
            {
                Root = 0,
                Directory = 1,
                File = 2
            }

            public PathNode(string text, NodeCategory category, PathNode parent = null)
            {
                Text = text;
                Category = category;
                Parent = parent;
                PathId = -1;
                Children = new ObservableCollection<PathNode>();
            }
        }
    }
}
