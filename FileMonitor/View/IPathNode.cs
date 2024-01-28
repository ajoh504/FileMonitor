using System.Collections.ObjectModel;

namespace FileMonitor.View
{
    public interface IPathNode
    {
        public string? Text { get; set; }
        public FileExplorerTreeView.NodeCategory Category { get; set; }
        public bool DisplayCheckBox { get; set; }
        public bool IsChecked { get; set; }
        public ObservableCollection<IPathNode>? Children { get; set; }
        public IPathNode Parent { get; set; }
        public int PathId { get; set; }
    }
}
