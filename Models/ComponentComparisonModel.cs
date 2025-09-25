using System.Collections.Generic;
using System.Xml.Linq;

namespace ComponentDiffEditor.Models
{
    public class ComponentFile
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public XElement? Component { get; set; }
        public double Similarity { get; set; }
        public bool IsSelected { get; set; }
        public List<Difference> Differences { get; set; } = new();
        public string FormattedSimilarity => $"{Similarity:F1}%";
        public string DisplayName => System.IO.Path.GetFileNameWithoutExtension(FileName);
    }

    public class Difference
    {
        public string Path { get; set; } = string.Empty;
        public string DefaultValue { get; set; } = string.Empty;
        public string ActualValue { get; set; } = string.Empty;
        public DifferenceType Type { get; set; }
        public string TypeDisplay => Type.ToString();
    }

    public enum DifferenceType
    {
        Missing,
        Different,
        Extra,
        Identical
    }

    public class ComponentTemplate
    {
        public string Name { get; set; } = string.Empty;
        public XElement Template { get; set; } = null!;
        public Dictionary<string, object> Statistics { get; set; } = new();
    }
}