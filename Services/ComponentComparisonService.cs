using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ComponentDiffEditor.Models;

namespace ComponentDiffEditor.Services
{
    public class ComponentComparisonService
    {
        private readonly string[] _componentTypes = {
            "wing", "rates", "gouge_check", "pattern", "tool",
            "feed_rate_advanced", "mach_surf", "link"
        };

        public List<ComponentFile> CompareWithDefault(string componentType, string defaultComponentPath, string scriptFolderPath)
        {
            var defaultComponent = LoadDefaultComponent(defaultComponentPath);
            if (defaultComponent == null)
                return new List<ComponentFile>();

            var componentFiles = new List<ComponentFile>();
            var xmlFiles = Directory.GetFiles(scriptFolderPath, "*.xml");

            foreach (var xmlFile in xmlFiles)
            {
                try
                {
                    var doc = XDocument.Load(xmlFile);
                    var components = doc.Descendants(componentType).ToList();

                    if (components.Any())
                    {
                        // 첫 번째 컴포넌트를 대표로 사용 (추후 개선 가능)
                        var component = components.First();
                        var similarity = CalculateSimilarity(defaultComponent, component);
                        var differences = FindDifferences(defaultComponent, component);

                        componentFiles.Add(new ComponentFile
                        {
                            FileName = Path.GetFileName(xmlFile),
                            FilePath = xmlFile,
                            Component = component,
                            Similarity = similarity,
                            Differences = differences,
                            IsSelected = similarity < 90 // 90% 미만인 것들은 기본 선택
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {xmlFile}: {ex.Message}");
                }
            }

            return componentFiles.OrderByDescending(x => x.Similarity).ToList();
        }

        private XElement? LoadDefaultComponent(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                var doc = XDocument.Load(filePath);
                return doc.Root;
            }
            catch
            {
                return null;
            }
        }

        public double CalculateSimilarity(XElement defaultComp, XElement actualComp)
        {
            var defaultNodes = GetAllLeafNodes(defaultComp);
            var actualNodes = GetAllLeafNodes(actualComp);

            if (!defaultNodes.Any() && !actualNodes.Any())
                return 100.0;

            if (!defaultNodes.Any() || !actualNodes.Any())
                return 0.0;

            int matches = 0;
            int totalComparisons = 0;

            foreach (var defaultNode in defaultNodes)
            {
                totalComparisons++;
                if (actualNodes.TryGetValue(defaultNode.Key, out var actualValue))
                {
                    if (IsValueSimilar(defaultNode.Value, actualValue))
                    {
                        matches++;
                    }
                }
            }

            // 추가 노드들에 대한 페널티
            var extraNodes = actualNodes.Keys.Except(defaultNodes.Keys).Count();
            totalComparisons += extraNodes;

            return totalComparisons > 0 ? (double)matches / totalComparisons * 100.0 : 0.0;
        }

        private Dictionary<string, string> GetAllLeafNodes(XElement element)
        {
            var nodes = new Dictionary<string, string>();
            GetLeafNodesRecursive(element, string.Empty, nodes);
            return nodes;
        }

        private void GetLeafNodesRecursive(XElement element, string path, Dictionary<string, string> nodes)
        {
            var currentPath = string.IsNullOrEmpty(path) ? element.Name.LocalName : $"{path}/{element.Name.LocalName}";

            if (!element.HasElements)
            {
                nodes[currentPath] = element.Value;
            }
            else
            {
                foreach (var child in element.Elements())
                {
                    GetLeafNodesRecursive(child, currentPath, nodes);
                }
            }
        }

        private bool IsValueSimilar(string val1, string val2)
        {
            if (string.IsNullOrEmpty(val1) && string.IsNullOrEmpty(val2))
                return true;

            if (string.IsNullOrEmpty(val1) || string.IsNullOrEmpty(val2))
                return false;

            // 숫자는 5% 오차 허용
            if (double.TryParse(val1, out var d1) && double.TryParse(val2, out var d2))
            {
                if (Math.Max(d1, d2) == 0) return d1 == d2;
                return Math.Abs(d1 - d2) / Math.Max(Math.Abs(d1), Math.Abs(d2)) < 0.05;
            }

            // 문자열은 대소문자 무시하고 비교
            return val1.Equals(val2, StringComparison.OrdinalIgnoreCase);
        }

        public List<Difference> FindDifferences(XElement defaultComp, XElement actualComp)
        {
            var differences = new List<Difference>();
            var defaultNodes = GetAllLeafNodes(defaultComp);
            var actualNodes = GetAllLeafNodes(actualComp);

            // 기본값과 다른 경우
            foreach (var defaultNode in defaultNodes)
            {
                if (actualNodes.TryGetValue(defaultNode.Key, out var actualValue))
                {
                    if (IsValueSimilar(defaultNode.Value, actualValue))
                    {
                        differences.Add(new Difference
                        {
                            Path = defaultNode.Key,
                            DefaultValue = defaultNode.Value,
                            ActualValue = actualValue,
                            Type = DifferenceType.Identical
                        });
                    }
                    else
                    {
                        differences.Add(new Difference
                        {
                            Path = defaultNode.Key,
                            DefaultValue = defaultNode.Value,
                            ActualValue = actualValue,
                            Type = DifferenceType.Different
                        });
                    }
                }
                else
                {
                    differences.Add(new Difference
                    {
                        Path = defaultNode.Key,
                        DefaultValue = defaultNode.Value,
                        ActualValue = "(missing)",
                        Type = DifferenceType.Missing
                    });
                }
            }

            // 추가 필드들
            foreach (var actualNode in actualNodes)
            {
                if (!defaultNodes.ContainsKey(actualNode.Key))
                {
                    differences.Add(new Difference
                    {
                        Path = actualNode.Key,
                        DefaultValue = "(not in default)",
                        ActualValue = actualNode.Value,
                        Type = DifferenceType.Extra
                    });
                }
            }

            return differences.OrderBy(d => d.Path).ToList();
        }

        public void ApplyDefaultToFile(XElement defaultComponent, string targetFilePath, string componentType)
        {
            var doc = XDocument.Load(targetFilePath);
            var existingComponents = doc.Descendants(componentType).ToList();

            if (existingComponents.Any())
            {
                // 첫 번째 컴포넌트를 기본값으로 교체
                var targetComponent = existingComponents.First();
                targetComponent.ReplaceWith(new XElement(defaultComponent));

                // 백업 생성
                var backupPath = targetFilePath + ".backup";
                if (!File.Exists(backupPath))
                {
                    File.Copy(targetFilePath, backupPath);
                }

                doc.Save(targetFilePath);
            }
        }

        public string[] GetAvailableComponentTypes() => _componentTypes;

        public List<string> GetDefaultComponentPaths(string defaultComponentsFolder)
        {
            if (!Directory.Exists(defaultComponentsFolder))
                return new List<string>();

            return Directory.GetFiles(defaultComponentsFolder, "*_default.xml")
                .ToList();
        }
    }
}