namespace Ivy.Blades;

public static class BladeHelper
{
    public static object WithHeader(object header, object content)
    {
        return new Fragment()
            | new Slot("BladeHeader", header)
            | content;
    }

}