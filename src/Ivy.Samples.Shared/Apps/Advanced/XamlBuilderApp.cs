namespace Ivy.Samples.Shared.Apps.Advanced;

[App(icon: Icons.Code, path: ["Advanced"])]
public class XamlBuilderApp : ViewBase
{
    private static readonly Dictionary<string, string> Examples = new()
    {
        ["Simple Layout"] = """<StackLayout Orientation="Vertical"><Badge Title="Hello" /><Badge Title="World" Variant="Success" /></StackLayout>""",
        ["Buttons"] = """<StackLayout Orientation="Horizontal" Gap="2"><Button Title="Primary" Variant="Primary" /><Button Title="Secondary" Variant="Secondary" /><Button Title="Destructive" Variant="Destructive" /></StackLayout>""",
        ["Card"] = """<Card><Badge Title="Content inside a card" /></Card>""",
        ["Nested Layout"] = """<StackLayout Orientation="Vertical" Gap="4"><StackLayout Orientation="Horizontal" Gap="2"><Badge Title="A" /><Badge Title="B" /></StackLayout><StackLayout Orientation="Horizontal" Gap="2"><Badge Title="C" Variant="Destructive" /><Badge Title="D" Variant="Success" /></StackLayout></StackLayout>""",
        ["Line Chart"] = """
            <LineChart>
              <Data><![CDATA[
                [
                  { "Month": "Jan", "Revenue": 4000, "Costs": 2400 },
                  { "Month": "Feb", "Revenue": 3000, "Costs": 1398 },
                  { "Month": "Mar", "Revenue": 5000, "Costs": 3200 },
                  { "Month": "Apr", "Revenue": 4780, "Costs": 3908 },
                  { "Month": "May", "Revenue": 5890, "Costs": 4800 },
                  { "Month": "Jun", "Revenue": 6390, "Costs": 3800 }
                ]
              ]]></Data>
              <LineChart.Lines>
                <Line DataKey="Revenue" />
                <Line DataKey="Costs" />
              </LineChart.Lines>
              <LineChart.XAxis>
                <XAxis DataKey="Month" />
              </LineChart.XAxis>
              <LineChart.YAxis>
                <YAxis />
              </LineChart.YAxis>
              <LineChart.CartesianGrid StrokeDashArray="3 3" />
              <LineChart.Tooltip />
              <LineChart.Legend Layout="Horizontal" Align="Center" />
            </LineChart>
            """,
        ["Bar Chart"] = """
            <BarChart>
              <Data><![CDATA[
                [
                  { "Name": "Page A", "Uv": 4000, "Pv": 2400 },
                  { "Name": "Page B", "Uv": 3000, "Pv": 1398 },
                  { "Name": "Page C", "Uv": 2000, "Pv": 9800 },
                  { "Name": "Page D", "Uv": 2780, "Pv": 3908 }
                ]
              ]]></Data>
              <BarChart.Bars>
                <Bar DataKey="Uv" />
                <Bar DataKey="Pv" />
              </BarChart.Bars>
              <BarChart.XAxis>
                <XAxis DataKey="Name" />
              </BarChart.XAxis>
              <BarChart.YAxis>
                <YAxis />
              </BarChart.YAxis>
              <BarChart.CartesianGrid StrokeDashArray="3 3" />
              <BarChart.Tooltip />
            </BarChart>
            """,
        ["Area Chart"] = """
            <AreaChart>
              <Data><![CDATA[
                [
                  { "Month": "Jan", "Desktop": 186, "Mobile": 80 },
                  { "Month": "Feb", "Desktop": 305, "Mobile": 200 },
                  { "Month": "Mar", "Desktop": 237, "Mobile": 120 },
                  { "Month": "Apr", "Desktop": 73, "Mobile": 190 },
                  { "Month": "May", "Desktop": 209, "Mobile": 130 },
                  { "Month": "Jun", "Desktop": 214, "Mobile": 140 }
                ]
              ]]></Data>
              <AreaChart.Areas>
                <Area DataKey="Desktop" CurveType="Natural" FillOpacity="0.4" />
                <Area DataKey="Mobile" CurveType="Natural" FillOpacity="0.4" />
              </AreaChart.Areas>
              <AreaChart.XAxis>
                <XAxis DataKey="Month" />
              </AreaChart.XAxis>
              <AreaChart.YAxis>
                <YAxis />
              </AreaChart.YAxis>
              <AreaChart.CartesianGrid StrokeDashArray="3 3" />
              <AreaChart.Tooltip />
            </AreaChart>
            """,
        ["Pie Chart"] = """
            <PieChart>
              <Data><![CDATA[
                [
                  { "Name": "Chrome", "Value": 275 },
                  { "Name": "Safari", "Value": 200 },
                  { "Name": "Firefox", "Value": 187 },
                  { "Name": "Edge", "Value": 173 },
                  { "Name": "Other", "Value": 90 }
                ]
              ]]></Data>
              <PieChart.Pies>
                <Pie DataKey="Value" NameKey="Name" />
              </PieChart.Pies>
              <PieChart.Tooltip />
              <PieChart.Legend Layout="Horizontal" />
            </PieChart>
            """,
    };

    private static readonly string[] ExampleNames = [.. Examples.Keys];

    private const string DefaultXaml = """<StackLayout Orientation="Vertical"><Badge Title="Hello" /><Badge Title="World" Variant="Success" /></StackLayout>""";

    public override object? Build()
    {
        var xml = UseState(DefaultXaml);
        var selectedExample = UseState("Simple Layout");

        UseEffect(() => xml.Set(Examples[selectedExample.Value]), selectedExample);

        object preview;
        try
        {
            if (string.IsNullOrWhiteSpace(xml.Value))
            {
                preview = Text.Muted("Enter XML to see a preview");
            }
            else
            {
                var builder = new XamlBuilder();
                preview = builder.Build(xml.Value);
            }
        }
        catch (Exception ex)
        {
            preview = Callout.Error(ex.Message);
        }

        return Layout.Horizontal().Gap(4)
               | (Layout.Vertical().Gap(2).Width(Size.Half())
                  | selectedExample.ToSelectInput(ExampleNames)
                  | xml.ToCodeInput().Language(Languages.Xml).Height(Size.Full()))
               | (Layout.Vertical().Width(Size.Half())
                  | preview);
    }
}
