using Ivy.Shared;

namespace Ivy.Samples.Apps.Tests;

[App(icon: Icons.Code, path: ["Tests"], isVisible: true)]
public class TestApp : ViewBase
{
    public override object? Build()
    {
        var left = Layout.Vertical()
                   | Text.H1("H1")
                   | Text.H2("H2")
                   | Text.H3("H3")
                   | Text.H4("H4")
                   | Text.Markdown(
                       "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                   | Text.Markdown(
                       "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                   | new Code("var x = 1;", "csharp")
                   | Text.Markdown(
                       """"""
                       * Item 1
                       * Item 2
                       * Item 3       
                       """""")
                   | Text.Markdown(
                       "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")
                   | Text.Markdown(
                       """"""
                       1. Item 1
                       2. Item 2
                       3. Item 3       
                       """""")
                   ;

        var right = new Markdown(
                """
                # H1
                ## H2
                ### H3
                #### H4
                
                Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
                
                Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
                
                ```csharp
                var x = 1;
                ```
                
                * Item 1
                * Item 2
                * Item 3
                
                Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
                
                1. Item 1
                2. Item 2
                3. Item 3
                
                """)
            ;

        return Layout.Grid().Columns(2)
               | left
               | right;
    }

}
