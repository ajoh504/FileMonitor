using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileMonitor.View
{
    public class PathNode
    {
        public string? Text { get; set; }
        public NodeCategory Category { get; set; }
        public bool DisplayCheckBox { get; set; }
        public int Id { get; set; }
        public ObservableCollection<PathNode>? Children { get; set; }

        public override string? ToString() => Text;

        public enum NodeCategory
        {
            Root = 0,
            Directory = 1,
            File = 2
        }

        public PathNode(string text, NodeCategory category)
        {
            Text = text;
            Category = category;
        }
    }
}
