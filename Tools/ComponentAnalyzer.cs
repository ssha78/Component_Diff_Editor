using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

class Program
{
    static void Main(string[] args)
    {
        var analyzer = new ComponentAnalyzer();
        var scriptPath = @"C:\Users\SSHA\claude_code\my_project\script_generater\script";

        Console.WriteLine("🔍 Component Analyzer Starting...");
        Console.WriteLine($"📁 Script Path: {scriptPath}");

        analyzer.AnalyzeAllComponents(scriptPath);

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
}

public class ComponentAnalyzer
{
    private readonly string[] ComponentTypes = {
        "wing", "rates", "gouge_check", "pattern", "tool",
        "feed_rate_advanced", "mach_surf", "link"
    };

    public void AnalyzeAllComponents(string scriptPath)
    {
        var xmlFiles = Directory.GetFiles(scriptPath, "*.xml").ToList();
        Console.WriteLine($"📋 Found {xmlFiles.Count} XML files");

        foreach (var componentType in ComponentTypes)
        {
            Console.WriteLine($"\n🧩 Analyzing Component: {componentType}");
            AnalyzeComponent(componentType, xmlFiles);
        }

        GenerateDefaultComponents(xmlFiles);
    }

    private void AnalyzeComponent(string componentType, List<string> xmlFiles)
    {
        var components = new Dictionary<string, List<XElement>>();

        foreach (var xmlFile in xmlFiles)
        {
            try
            {
                var doc = XDocument.Load(xmlFile);
                var fileName = Path.GetFileNameWithoutExtension(xmlFile);

                var foundComponents = doc.Descendants(componentType).ToList();
                if (foundComponents.Any())
                {
                    components[fileName] = foundComponents;
                    Console.WriteLine($"  ✓ {fileName}: {foundComponents.Count} {componentType}(s)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Error reading {Path.GetFileName(xmlFile)}: {ex.Message}");
            }
        }

        if (components.Any())
        {
            GenerateComponentReport(componentType, components);
        }
        else
        {
            Console.WriteLine($"  ⚠️ No {componentType} components found");
        }
    }

    private void GenerateComponentReport(string componentType, Dictionary<string, List<XElement>> components)
    {
        var reportPath = $"component_report_{componentType}.txt";
        using var writer = new StreamWriter(reportPath);

        writer.WriteLine($"📊 Component Analysis Report: {componentType.ToUpper()}");
        writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine(new string('=', 50));

        // 공통 구조 분석
        var allElements = new Dictionary<string, int>();

        foreach (var fileComponents in components.Values)
        {
            foreach (var component in fileComponents)
            {
                foreach (var element in component.Elements())
                {
                    var elementName = element.Name.LocalName;
                    allElements[elementName] = allElements.GetValueOrDefault(elementName, 0) + 1;
                }
            }
        }

        writer.WriteLine("\n🔍 Element Frequency:");
        foreach (var element in allElements.OrderByDescending(x => x.Value))
        {
            writer.WriteLine($"  {element.Key}: {element.Value} occurrences");
        }

        // 파일별 세부사항
        writer.WriteLine("\n📋 File Details:");
        foreach (var file in components)
        {
            writer.WriteLine($"\n📄 {file.Key}:");
            foreach (var component in file.Value)
            {
                writer.WriteLine($"  Component #{file.Value.IndexOf(component) + 1}:");
                WriteComponentStructure(writer, component, "    ");
            }
        }

        Console.WriteLine($"  📝 Report saved: {reportPath}");
    }

    private void WriteComponentStructure(StreamWriter writer, XElement element, string indent)
    {
        foreach (var child in element.Elements())
        {
            if (child.HasElements)
            {
                writer.WriteLine($"{indent}{child.Name.LocalName}:");
                WriteComponentStructure(writer, child, indent + "  ");
            }
            else
            {
                writer.WriteLine($"{indent}{child.Name.LocalName}: {child.Value}");
            }
        }
    }

    private void GenerateDefaultComponents(List<string> xmlFiles)
    {
        Console.WriteLine("\n🏗️ Generating Default Components...");

        Directory.CreateDirectory("default_components");

        foreach (var componentType in ComponentTypes)
        {
            try
            {
                var defaultComponent = CreateDefaultComponent(componentType, xmlFiles);
                if (defaultComponent != null)
                {
                    var defaultPath = Path.Combine("default_components", $"{componentType}_default.xml");
                    var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), defaultComponent);
                    doc.Save(defaultPath);
                    Console.WriteLine($"  ✓ Created: {defaultPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ Error creating default {componentType}: {ex.Message}");
            }
        }
    }

    private XElement? CreateDefaultComponent(string componentType, List<string> xmlFiles)
    {
        var allComponents = new List<XElement>();

        foreach (var xmlFile in xmlFiles)
        {
            try
            {
                var doc = XDocument.Load(xmlFile);
                allComponents.AddRange(doc.Descendants(componentType));
            }
            catch
            {
                continue;
            }
        }

        if (!allComponents.Any())
            return null;

        return componentType switch
        {
            "rates" => CreateDefaultRates(allComponents),
            "wing" => CreateDefaultWing(allComponents),
            "tool" => CreateDefaultTool(allComponents),
            "gouge_check" => CreateDefaultGougeCheck(allComponents),
            "pattern" => CreateDefaultPattern(allComponents),
            _ => CreateGenericDefault(componentType, allComponents)
        };
    }

    private XElement CreateDefaultRates(List<XElement> components)
    {
        var feedValues = ExtractNumericValues(components, "feed");
        var plungeValues = ExtractNumericValues(components, "plunge");
        var retractValues = ExtractNumericValues(components, "retract");

        return new XElement("rates",
            new XElement("feed", CalculateMedian(feedValues)),
            new XElement("plunge", CalculateMedian(plungeValues)),
            new XElement("retract", CalculateMedian(retractValues)),
            new XElement("up_percentage", "100.0"),
            new XElement("down_percentage", "90.0")
        );
    }

    private XElement CreateDefaultWing(List<XElement> components)
    {
        var widthValues = ExtractNumericValues(components, "width");
        var offsetValues = ExtractNumericValues(components, "inside_offset");

        return new XElement("wing",
            new XElement("width", CalculateMedian(widthValues)),
            new XElement("inside_offset", CalculateMedian(offsetValues)),
            new XElement("tolerance", "0.01"),
            new XElement("silhouette_tool_diameter", "1.4"),
            new XElement("wing_tool",
                new XElement("external", "02")
            ),
            new XElement("main_mesh", "coping"),
            new XElement("pins_mesh", "pins"),
            new XElement("wings_mesh_name", "New_wings_cavity"),
            new XElement("bottom_external", "true")
        );
    }

    private XElement CreateDefaultTool(List<XElement> components)
    {
        var externalValues = ExtractStringValues(components, "external");
        var mostCommon = externalValues.GroupBy(x => x)
            .OrderByDescending(g => g.Count())
            .First().Key;

        return new XElement("tool",
            new XElement("external", mostCommon)
        );
    }

    private XElement CreateDefaultGougeCheck(List<XElement> components)
    {
        return new XElement("gouge_check",
            new XElement("status", "true"),
            new XElement("check_flute", "false"),
            new XElement("check_shaft", "true"),
            new XElement("check_surf", "pins"),
            new XElement("strategy", "retract_along_tool_axis"),
            new XElement("check_between_points", "true"),
            new XElement("check_links_collide", "true"),
            new XElement("tolerance", "0.01"),
            new XElement("thickness", "0"),
            new XElement("clearance_shaft", "0"),
            new XElement("clearance_arbor", "0"),
            new XElement("clearance_holder", "0")
        );
    }

    private XElement CreateDefaultPattern(List<XElement> components)
    {
        var stepoverValues = ExtractNumericValues(components, "stepover");

        return new XElement("pattern",
            new XElement("type", "ConstantCusp"),
            new XElement("stepover", CalculateMedian(stepoverValues)),
            new XElement("cut_order", "standard"),
            new XElement("cut_method", "zigzag")
        );
    }

    private XElement CreateGenericDefault(string componentType, List<XElement> components)
    {
        var firstComponent = components.First();
        return new XElement(firstComponent);
    }

    private List<double> ExtractNumericValues(List<XElement> components, string elementName)
    {
        var values = new List<double>();

        foreach (var component in components)
        {
            var element = component.Element(elementName);
            if (element != null && double.TryParse(element.Value, out var value))
            {
                values.Add(value);
            }
        }

        return values;
    }

    private List<string> ExtractStringValues(List<XElement> components, string elementName)
    {
        var values = new List<string>();

        foreach (var component in components)
        {
            var element = component.Element(elementName);
            if (element != null && !string.IsNullOrWhiteSpace(element.Value))
            {
                values.Add(element.Value);
            }
        }

        return values;
    }

    private double CalculateMedian(List<double> values)
    {
        if (!values.Any()) return 0;

        values.Sort();
        var count = values.Count;

        if (count % 2 == 0)
        {
            return (values[count / 2 - 1] + values[count / 2]) / 2.0;
        }
        else
        {
            return values[count / 2];
        }
    }
}