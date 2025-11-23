using Ivy.Core.Hooks;
using Ivy.Services;

namespace Ivy.Views.Forms;

public static class FormExtensions
{
    public static FormBuilder<T> ToForm<T>(this IState<T> obj, string submitTitle = "Save")
    {
        return new FormBuilder<T>(obj, submitTitle);
    }
}